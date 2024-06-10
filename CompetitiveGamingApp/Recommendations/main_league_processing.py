from data_league_preprocessing import *
from train_test_league_model import * 
from cachetools import LRUCache
import shelve
import pika
import requests
import json
from dataclasses import dataclass
import time

@dataclass
class LeagueInfo:
    LeagueName: str
    LeagueTags: list[str]
    LeagueIndividualOverallRecord: list[list[int]]


def send_response_back(player_uname, recommendations):
    connection = pika.BlockingConnection(pika.ConnectionParameters('rabbitmq'))
    channel = connection.channel()

    channel.queue_declare("recommendations_league_queue_" + player_uname)

    req = {
        "player" : player_uname,
        "recommendations" : recommendations
    }

    msg = json.dumps(req)

    channel.basic_publish(exchange='', routing_key="recommendations_league_queue_" + player_uname, body=msg)

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

    channel.queue_declare(queue='league_rec_notifications')

    channel.basic_consume(queue='league_rec_notifications', on_message_callback=callback, auto_ack=True)

    channel.start_consuming()

    channel.close()

    return player

def create_or_load_cache():
    try:
        new_cache = None
        with shelve.open("league_record.db") as cache_db:
            cache = cache_db.get("lru_cache")
            if cache:
                return cache
            
        new_cache = LRUCache(maxsize=400)
        with shelve.open("league_record.db", writeblock=True) as cache_db:
            cache_db["lru_cache"] = new_cache
    except (FileNotFoundError, shelve.ShelfError):
        pass

def get_available_leagues(records):
    all_leagues = requests.get("/leagues/")

    total_leagues = [league.LeagueId for league in all_leagues]

    used_leagues = [league.LeagueId for league in records]

    unused_leagues = list(set(total_leagues) - set(used_leagues))

    available_leagues = []

    for item in unused_leagues:
        current_league = requests.get("/League/${item}")

        all_player_records = []

        player_record_dict = {}

        for season in current_league.ArchieveLeagueStandings:
            for record in season.Table:
                if record["playerName"] in player_record_dict:
                    player_record_dict[record["playerName"]][0] += record["wins"]
                    player_record_dict[record["playerName"]][1] += record["losses"]
                else:
                    player_record_dict[record["playerName"]] = [record["wins"], record["losses"]]
        
        for record in player_record_dict.values():
            all_player_records.append(record)

        available_leagues.append(LeagueInfo(current_league.Name, current_league.Tags, all_player_records))

    return available_leagues

def RunLeagueRecommendationFlow(username, records):
    cache = create_or_load_cache()
    recommendations = []

    if username in cache:
        league_record_model = cache[username]
        recent_record = records[-1]
        all_tags = league_record_model.all_tags
        cond = lambda x: x.LeagueName == recent_record.Name
        mod_available_leagues = list(filter(lambda x : not cond(x), league_record_model.available_leagues))
        model, label_encoder = add_data(league_record_model.model, league_record_model.all_tags, league_record_model.label_encoder, recent_record)
        recommendations = recommend_leagues(model, mod_available_leagues, all_tags, label_encoder)

        cache[username] = {
            "model" : model,
            "label_encoder" : label_encoder,
            "all_tags" : all_tags,
            "available_leagues" : mod_available_leagues
        }

    else:
        model, label_encoder, all_tags = fit(records)

        available_leagues = get_available_leagues(records)
        recommendations = recommend_leagues(model, available_leagues, all_tags, label_encoder)

        league_analysis = {
            "model" : model,
            "label_encoder" : label_encoder,
            "all_tags" : all_tags,
            "available_leagues" : available_leagues
        }

        cache[username] = league_analysis
    
    with shelve.open("league_record.db") as cache_db:
        cache_db["lru_cache"] = cache

    return recommendations

def main():
    player = None
    player = recieve_notification(player)

    past_records = requests.get('/recommendations/League/${player}')

    recommendations = []
    if len(past_records) >= 5:
        recommendations = RunLeagueRecommendationFlow(player, past_records)
        send_response_back(player, recommendations)

if __name__ == "__main__":
    while True:
        main()
        time.sleep(60)
