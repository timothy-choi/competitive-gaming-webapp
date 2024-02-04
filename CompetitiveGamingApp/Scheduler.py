from flask import Flask, request, jsonify
from datetime import datetime, timedelta
import random

app = Flask(__name__)

def GetValidPlayers(players, do_not_play, current):
   return [player for player in players if player not in do_not_play[current] and player != current]

def GetOpenGames(player_schedule, current):
   return [index for index, game in enumerate(player_schedule) if game is None]

def FindOpponentPosition(players, name):
   for index, player in enumerate(players):
      if name == player:
         return index

def SolvePlayerScheduleDivisions(players, num_games, groups):
    pass

def SolvePlayerScheduleWhole(players, num_games, do_not_play, min_repeat_times, max_repeat_times, start_dates, interval_between_games, interval_between_games_hours):
   schedule_table = [[None] * len(players) for _ in range(num_games)]
   for index1, player_schedule in enumerate(schedule_table):
    for index2, opponent in enumerate(player_schedule):
         if opponent is not None:
            continue
         other_players = GetValidPlayers(players, do_not_play, players[index1])

         selected_player = random.choice(other_players)

         times_to_play = 0

         if min_repeat_times != -1 and max_repeat_times != -1:
            times_to_play = random.randint(min_repeat_times, max_repeat_times)
         else:
            times_to_play = random.randint(1, max_repeat_times)
        
         if times_to_play > 1:
        
            open_slots = GetOpenGames(player_schedule, index2)

            times = random.sample(open_slots, times_to_play)

            for time in times:
               schedule_table[index1][time] = selected_player

               player_index = FindOpponentPosition(players, selected_player)

               schedule_table[player_index][index2] = players[index1]
         
         else:
        
            schedule_table[index1][index2] = selected_player

            player_index = FindOpponentPosition(players, selected_player)

            schedule_table[player_index][index2] = players[index1]
    
    total_schedule = {}
   
    for index, player in enumerate(schedule_table):
      game_date = start_dates[players[index]]
      total_schedule[players[index]] = []
      for index, game in enumerate(player):
         if index > 0:
            game_date += timedelta(days=interval_between_games, hours=interval_between_games_hours)
         game_pair = [game, game_date]
         total_schedule[players[index]].insert(game_pair)
    
    return total_schedule


@app.route('/Schedules', methods=['POST'])
def GeneratePlayerSchedule():
  all_player_data = request.json()

  player_schedules = {}

  if all_player_data.get("whole_mode"):
    player_schedules = SolvePlayerScheduleWhole()
  else:
    player_schedules = SolvePlayerScheduleDivisions()
  
  return jsonify({"schedules": player_schedules})
