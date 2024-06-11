from CompetitiveGamingApp.Recommendations.data_user_preprocessing import load_data, process_data
from train_test_user_model import *
import requests
import time
import pika
from cachetools import LRUCache
import shelve
import json
from dataclasses import dataclass

@dataclass
class AvailablePlayer:
    Username: str
    joined_league: bool
    League: str
    league_tags: list[str]
    record: list[int]


def get_unused_players(records):
    all_players = requests.get("Players")

    total_players = [player.playerUsername for player in all_players]

    all_played_users = [player.playerUsername for player in records]

    available_players = list(set(total_players) - set(all_played_users))

    unused_players = []

    for player in available_players:
        player_info = requests.get("/Player/${player}")

        all_leagues = requests.get("/Leagues")

        league_tags = []

        for league in all_leagues:
            if league.Name == player_info.LeagueName:
                league_tags = league.tags
                break

        unused_players.append(AvailablePlayer(player, player_info.joined_league, player_info.LeagueName, league_tags, player_info.singlePlayerRecord))
    
    return unused_players


def create_or_load_cache():
    try:
        new_cache = None
        with shelve.open("user_record.db") as cache_db:
            cache = cache_db.get("lru_cache")
            if cache:
                return cache
            
        new_cache = LRUCache(maxsize=400)
        with shelve.open("user_record.db", writeblock=True) as cache_db:
            cache_db["lru_cache"] = new_cache
    except (FileNotFoundError, shelve.ShelfError):
        pass

def send_response_back(player_uname, recommendations):
    connection = pika.BlockingConnection(pika.ConnectionParameters('rabbitmq'))
    channel = connection.channel()

    channel.queue_declare("recommendations_queue_" + player_uname)

    req = {
        "player" : player_uname,
        "recommendations" : recommendations
    }

    msg = json.dumps(req)

    channel.basic_publish(exchange='', routing_key="recommendations_queue_" + player_uname, body=msg)

    connection.close()

def recieve_notification(player):
    def callback(ch, method, properties, body):
        msg = body.decode('utf-8')
        hyphen_index = msg.find('-')
        if hyphen_index != -1:
            player = msg[:hyphen_index]
        else:
            player = msg
        channel.stop_consuming()

    connection = pika.BlockingConnection(pika.ConnectionParameters('rabbitmq'))

    channel = connection.channel()

    channel.queue_declare(queue='player_rec_notifications')

    channel.basic_consume(queue='player_rec_notifications', on_message_callback=callback, auto_ack=True)

    channel.start_consuming()

    channel.close()

    return player


def RunUserRecommendationModelFlow(player_uname, player_records):
    cache = create_or_load_cache()
    player_record_class = None
    recommendations = []
    if player_uname in cache:
        player_record_class = cache[player_uname]
        recent_record = player_records[-1]
        current_class =  player_record_class[0]
        user_matrix = player_record_class[1]
        cond = lambda x: x.playerUsername == recent_record.playerUsername
        unplayed_users = list(filter(lambda x : not cond(x), player_record_class[2]))
        user_matrix = current_class.add_data(player_uname, recent_record.PlayerLeague, recent_record.PlayerRecord[0], recent_record.PlayerRecord[1], recent_record.PlayerLeagueTags, user_matrix)
        recommendations = current_class.recommend(current_class.user_ids[player_uname], user_matrix, unplayed_users)
        cache[player_uname] = [current_class, user_matrix, unplayed_users]
    else:
        dframe = load_data(player_records)
        dframe, X, y = process_data(dframe)
        user_matrix, user_ids, league_ids = create_combined_matrix(dframe, X)
        userClass = UserIncrementalSVD()
        userClass.fit(user_matrix)
        user_idx = user_ids[player_uname]
        unplayed_users = get_unused_players(player_records)
        recommendations = userClass.recommend(user_idx, user_matrix, unplayed_users)

        cache[player_uname] = [userClass, user_matrix, unplayed_users]

    with shelve.open("user_record.db") as cache_db:
        cache_db["lru_cache"] = cache

    return recommendations

def main():
    player_uname = recieve_notification(player_uname)

    user_records = requests.get('/recommendations/Player/${player_uname}')

    if len(user_records.PlayerHistoryRecords) >= 10:
        recommendations = RunUserRecommendationModelFlow(player_uname, user_records.PlayerHistoryRecords)
        send_response_back(player_uname, recommendations)

if __name__ == "__main__":
    while True:
        main()
        time.sleep(60)