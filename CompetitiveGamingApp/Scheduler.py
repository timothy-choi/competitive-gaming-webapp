from flask import Flask, request, jsonify
from datetime import datetime, timedelta
import random

app = Flask(__name__)

def GetValidPlayers(players, do_not_play, current):
   return [player for player in players if player not in do_not_play[current] and player != current]

def GetOpenGames(player_schedule, current):
   return [index for index, game in enumerate(player_schedule) if game is None and index > current]

def FindOpponentPosition(players, name):
   for index, player in enumerate(players):
      if name == player:
         return index

def SolvePlayerScheduleDivisions(players, num_games, groups):
    pass

def SolvePlayerWholeScheduleAllPlayers(players, num_games, min_repeat_times, max_repeat_times, start_dates, interval_between_games, interval_between_games_hours):
   if num_games < len(players) * min_repeat_times or num_games > len(players) * max_repeat_times:
      return None
   schedule_table = [[None] * len(num_games) for _ in range(players)]
   for index1, player_schedule in enumerate(schedule_table):
      used_players = [player for player in player_schedule if player is not None and player_schedule.count(player) >= min_repeat_times]
      available_players = [player for player in players if player not in used_players]
      index = 0
      while len(available_players) > 0:
         other_player = random.choice(available_players)
         available_players.remove(other_player)
         if min_repeat_times > 1:
            times_played = min_repeat_times
            time_slots = GetOpenGames(player_schedule, index)
            selected_slots = random.sample(time_slots, times_played)
            for spot in selected_slots:
               schedule_table[index1][spot] = other_player
               opponent_spot = FindOpponentPosition(players, other_player)
               schedule_table[opponent_spot][spot] = players[index1]
         else:
            times_played = 1
            schedule_table[index1][index] = other_player
            opponent_spot = FindOpponentPosition(players, other_player)
            schedule_table[opponent_spot][spot] = players[index1]
            
      available_spots = [index for index in range(player_schedule) if player_schedule[index] is None]
      available_players = [player for player in players if player != players[index1]]
      if len(available_spots) > 0 and max_repeat_times == min_repeat_times + 1:
         return None 
      for spot in available_spots:
         if len(available_players) == 0:
            if max_repeat_times <= min_repeat_times + 1:
               return None
            else:
               available_players = [player for player in players if player != players[index1]]
         opponent = random.choice(available_players)
         available_players.remove(opponent)
         schedule_table[index1][index] = opponent
         opponent_spot = FindOpponentPosition(players, opponent)
         schedule_table[opponent_spot][spot] = players[index1]

   total_schedule = {}

   sideOpts = ['H', 'A']
   
   for index, player in enumerate(schedule_table):
     game_date = start_dates[players[index]]
     total_schedule[players[index]] = []
     for index, game in enumerate(player):
        if index > 0:
          game_date += timedelta(days=interval_between_games, hours=interval_between_games_hours)
          loc = random.choice(sideOpts)
          game_pair = [game, game_date, loc]
          total_schedule[players[index]].insert(game_pair)
    
   return total_schedule




def SolvePlayerScheduleWhole(players, num_games, do_not_play, min_repeat_times, max_repeat_times, start_dates, interval_between_games, interval_between_games_hours):
   schedule_table = [[None] * len(num_games) for _ in range(players)]
   for index1, player_schedule in enumerate(schedule_table):
    for index2, opponent in enumerate(player_schedule):
         if opponent is not None:
            continue
         other_players = GetValidPlayers(players, do_not_play, players[index1])

         selected_player = random.choice(other_players)

         times_to_play = 0

         if min_repeat_times > 1:
            times_to_play = random.randint(min_repeat_times, max_repeat_times)
         else:
            times_to_play = random.randint(1, max_repeat_times)
        
         if times_to_play > 1:
        
            open_slots = GetOpenGames(player_schedule, index2)

            times = random.sample(open_slots, times_to_play)

            schedule_table[index1][index2] = selected_player

            player_index = FindOpponentPosition(players, selected_player)

            schedule_table[player_index][index2] = players[index1]

            for time in times:
               schedule_table[index1][time] = selected_player

               player_index = FindOpponentPosition(players, selected_player)

               schedule_table[player_index][index2] = players[index1]
         
         else:
        
            schedule_table[index1][index2] = selected_player

            player_index = FindOpponentPosition(players, selected_player)

            schedule_table[player_index][index2] = players[index1]
    
    total_schedule = {}

    sideOpts = ['H', 'A']
   
    for index, player in enumerate(schedule_table):
      game_date = start_dates[players[index]]
      total_schedule[players[index]] = []
      for index, game in enumerate(player):
         if index > 0:
            game_date += timedelta(days=interval_between_games, hours=interval_between_games_hours)
         loc = random.choice(sideOpts)
         game_pair = [game, game_date, loc]
         total_schedule[players[index]].insert(game_pair)
    
    return total_schedule


@app.route('/Schedules', methods=['POST'])
def GeneratePlayerSchedule():
  all_player_data = request.json()

  player_schedules = {}

  if all_player_data.get("whole_mode"):
    if all_player_data.get("playAllTeams"):
        player_schedules = SolvePlayerWholeScheduleAllPlayers()
    else:
        player_schedules = SolvePlayerScheduleWhole()
  else:
    player_schedules = SolvePlayerScheduleDivisions()
  
  return jsonify({"schedules": player_schedules})
