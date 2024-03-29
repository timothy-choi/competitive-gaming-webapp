from flask import Flask, request, jsonify
from datetime import datetime, timedelta
import random
import json

app = Flask(__name__)

def GetValidPlayers(players, do_not_play, current):
   return [player for player in players if player not in do_not_play[current] and player != current]

def GetOpenGames(player_schedule, current):
   return [index for index, game in enumerate(player_schedule) if game is None and index > current]

def FindOpponentPosition(players, name):
   for index, player in enumerate(players):
      if name == player:
         return index

def SolvePlayerScheduleDivisions(players, num_games, groups, outside_groups, player_groups, outside_player_limit, max_repeat_outside_matches, start_dates, interval_between_games, interval_between_games_hours, exclude_outside_divisions, repeat_matchups):
    schedule_table = [[None] * len(num_games) for _ in range(players)]
    if not exclude_outside_divisions:
       for index1, player_schedule in enumerate(schedule_table):
            all_outside_players = [player for key, value in groups.items() if key in outside_groups[players[index1]] for person in value]
            if not repeat_matchups and outside_player_limit > len(all_outside_players):
               return None
            open_spots_left = outside_player_limit - sum(1 for elt in player_schedule if elt is not None)
            index2 = 0
            while open_spots_left > 0:
               if player_schedule[index1][index2] is not None:
                  index2 += 1
                  continue
               opponent = random.choice(all_outside_players)
               all_outside_players.remove(opponent)
               total_times_played = random.randint(1, max_repeat_outside_matches) 
               if open_spots_left < total_times_played:
                  total_times_played = open_spots_left
               open_spots_left -= total_times_played
               open_slots = GetOpenGames(player_schedule, index2)
               random_open_slots = random.sample(open_slots, total_times_played-1)
               random_open_slots.insert(0, index2)
               for slot in random_open_slots:
                  sideOpts = ['H', 'A']
                  loc = random.choices(sideOpts)
                  sideOpts.remove(loc)
                  schedule_table[index1][slot] = [opponent, loc]
                  opponent_spot = FindOpponentPosition(players, opponent)
                  schedule_table[opponent_spot][slot] = [index1, slot]
               index2 += 1
               
    
    for index1, player_schedule in enumerate(schedule_table):
       for index2, player in enumerate(player_schedule):
          if player is not None:
             continue
          opponent = random.choice(player_groups[players[index1]])

          sideOpts = ['H', 'A']
          loc = random.choices(sideOpts)
          sideOpts.remove(loc)

          schedule_table[index1][index2] = [opponent, loc]

          opponent_pos = FindOpponentPosition(players, opponent)

          schedule_table[opponent_pos][index2] = [index1, index2]

    total_schedule = {}
   
    for index, player in enumerate(schedule_table):
      game_date = start_dates[players[index]]
      total_schedule[players[index]] = []
      for index, game in enumerate(player):
         if index > 0:
           game_date += timedelta(days=interval_between_games, hours=interval_between_games_hours)
         if isinstance(game[0], int):
            game_pair = [game[0], game[1]]
         else:
            game_pair = [game[0], game_date, game[1]]
         total_schedule[players[index]].insert(game_pair)
    
    return total_schedule


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
            selected_slots = random.sample(time_slots, times_played-1)
            selected_slots.insert(0, index)
            for spot in selected_slots:
               sideOpts = ['H', 'A']
               loc = random.choices(sideOpts)
               sideOpts.remove(loc)
               schedule_table[index1][spot] = [other_player, loc]
               opponent_spot = FindOpponentPosition(players, other_player)
               schedule_table[opponent_spot][spot] = [index1, spot]
         else:
            sideOpts = ['H', 'A']
            loc = random.choices(sideOpts)
            sideOpts.remove(loc)
            schedule_table[index1][index] = [other_player, loc]
            opponent_spot = FindOpponentPosition(players, other_player)
            schedule_table[opponent_spot][index] = [index1, index]
         index = index + 1
            
      available_spots = [index for index in range(player_schedule) if player_schedule[index] is None]
      available_players = [player for player in players if player != players[index1]]
      if len(available_spots) > 0 and max_repeat_times <= min_repeat_times + 1:
         return None 
      for spot in available_spots:
         if len(available_players) == 0:
            if max_repeat_times <= min_repeat_times + 1:
               return None
            else:
               available_players = [player for player in players if player != players[index1]]
         opponent = random.choice(available_players)
         available_players.remove(opponent)
         sideOpts = ['H', 'A']
         loc = random.choices(sideOpts)
         sideOpts.remove(loc)
         schedule_table[index1][spot] = [opponent, loc]
         opponent_spot = FindOpponentPosition(players, opponent)
         schedule_table[opponent_spot][spot] = [index1, spot]

   total_schedule = {}
   
   for index, player in enumerate(schedule_table):
     game_date = start_dates[players[index]]
     total_schedule[players[index]] = []
     for index, game in enumerate(player):
        if index > 0:
          game_date += timedelta(days=interval_between_games, hours=interval_between_games_hours)
        if isinstance(game[0], int):
           game_pair = [game[0], game[1]]
        else:
           game_pair = [game[0], game_date, game[1]]
        total_schedule[players[index]].insert(game_pair)
    
   return total_schedule




def SolvePlayerScheduleWhole(players, num_games, do_not_play, min_repeat_times, max_repeat_times, start_dates, interval_between_games, interval_between_games_hours):
   if num_games < len(players) * min_repeat_times or num_games > len(players) * max_repeat_times:
      return None
   schedule_table = [[None] * len(num_games) for _ in range(players)]
   leftover_games = {}
   excess = False
   for index1, player_schedule in enumerate(schedule_table):
    other_players = GetValidPlayers(players, do_not_play, players[index1])
    for index2, opponent in enumerate(player_schedule):
         if opponent is not None:
            continue
         if len(other_players) == 0:
            excess = True
            for[player, leftovers] in leftover_games[players[index1]]:
               if leftovers > 0:
                  selected_player = player
                  if leftovers <= sum(1 for elt in player_schedule if elt is None):
                     times_to_play = leftovers
                  else:
                     times_to_play = sum(1 for elt in player_schedule if elt is None)
                  leftover_games[players[index1]][player] = 0
         else:
            selected_player = random.choice(other_players)
            other_players.remove(selected_player)

         if not excess:
            times_to_play = 0

         if min_repeat_times > 1 and not excess:
            times_to_play = random.randint(min_repeat_times, max_repeat_times)
         elif min_repeat_times < 1 and not excess:
            times_to_play = random.randint(1, max_repeat_times)
         
         if not excess:
            leftover_games[players[index1]].append([selected_player, max_repeat_times - times_to_play])
         if times_to_play > 1:
        
            open_slots = GetOpenGames(player_schedule, index2)

            if times_to_play >= len(open_slots):
               times_to_play = len(open_slots)

            times = random.sample(open_slots, times_to_play-1)

            times.insert(0, index2)

            for time in times:
               sideOpts = ['H', 'A']
               loc = random.choices(sideOpts)
               sideOpts.remove(loc)
               schedule_table[index1][time] = [selected_player, loc]

               player_index = FindOpponentPosition(players, selected_player)

               schedule_table[player_index][time] = [index1, time]
         
         else:
            sideOpts = ['H', 'A']
            loc = random.choices(sideOpts)
            sideOpts.remove(loc)
        
            schedule_table[index1][index2] = [selected_player, loc]

            player_index = FindOpponentPosition(players, selected_player)

            schedule_table[player_index][index2] = [index1, index2]
    
    total_schedule = {}
   
    for index, player in enumerate(schedule_table):
      game_date = start_dates[players[index]]
      total_schedule[players[index]] = []
      for index, game in enumerate(player):
         if index > 0:
            game_date += timedelta(days=interval_between_games, hours=interval_between_games_hours)
         if isinstance(game[0], int):
            game_pair = [game[0], game[1]]
         else:
            game_pair = [game[0], game_date, game[1]]
         total_schedule[players[index]].insert(game_pair)
    
    return total_schedule


@app.route('/Schedules', methods=['POST'])
def GeneratePlayerSchedule():
  all_player_data = request.json()

  player_schedules = {}

  player_schedule_info = json.loads(all_player_data)


  if player_schedule_info["whole_mode"]:
    if player_schedule_info["playAllTeams"]:
        player_schedules = SolvePlayerWholeScheduleAllPlayers(player_schedule_info["players"], player_schedule_info["num_games"], player_schedule_info["min_repeat_times"], 
                                                              player_schedule_info["max_repeat_times"], player_schedule_info["start_dates"], player_schedule_info["interval_between_games"], 
                                                              player_schedule_info["interval_between_games_hours"])
    else:
        player_schedules = SolvePlayerScheduleWhole(player_schedule_info["players"], player_schedule_info["num_games"], player_schedule_info["do_not_play"], 
                                                    player_schedule_info["min_repeat_times"], player_schedule_info["max_repeat_times"], player_schedule_info["start_dates"], 
                                                    player_schedule_info["interval_between_games"], player_schedule_info["interval_between_games_hours"])
  else:
      if player_schedule_info["playAllTeams"]:
         player_schedules = SolvePlayerWholeScheduleAllPlayers(player_schedule_info["players"], player_schedule_info["num_games"], player_schedule_info["min_repeat_times"], 
                                                              player_schedule_info["max_repeat_times"], player_schedule_info["start_dates"], player_schedule_info["interval_between_games"], 
                                                              player_schedule_info["interval_between_games_hours"])
      else:
         player_schedules = SolvePlayerScheduleDivisions(player_schedule_info["players"], player_schedule_info["num_games"], player_schedule_info["groups"], player_schedule_info["outside_groups"], 
                                                       player_schedule_info["player_groups"], player_schedule_info["outside_player_limit"], player_schedule_info["max_repeat_outside_matches"],
                                                       player_schedule_info["start_dates"], player_schedule_info["interval_between_games"], player_schedule_info["interval_between_games_hours"], 
                                                       player_schedule_info["exclude_outside_divisions"], player_schedule_info["repeat_matchups"])
  
  return jsonify({"schedules": player_schedules})