import { React, useRef, useState, useEffect, useContext } from 'react';
import axios from './api/axios';
import { useLocation, useHistory} from 'react-router-dom'; 
import {useAuth} from "./context/AuthProvider";

const SearchResults = () => {
    const {loggedIn, username } = useAuth();

    const location = useLocation();

    const history = useHistory();

    const [query, setQuery] = useState('');

    const [playerResults, setPlayerResults] = setState([]);

    const [gameResults, setGameResults] = setState([]);

    const [leagueResults, setLeagueResults] = setState([]);

    useEffect(() => {
        const params = new URLSearchParams(location.search);
        const q = params.get('query');

        const fetchAllResults = async (q) => {
            const playerRes = await axios.get(`/Search/Player/${q}`);

            var playerResultCopy = [];

            for (var player in playerRes.data) {
                var res = await axios.get(`/Player/${player.PlayerId}`);
                var playerInfo = {
                    Id: player.PlayerId,
                    Name: player.Name,
                    Username: player.Username,
                    IsAvailable: res.data.isAvailable,
                    GameStatus: res.data.playerInGame,
                    record: res.data.singlePlayerRecord
                };

                playerResultCopy.push(playerInfo);
            }

            setPlayerResults(playerResultCopy);

            const gameRes = await axios.get(`/Search/Game/${q}`);

            var gameResultsCopy = [];

            for (var game in gameRes.data) {
                var res = await axios.get(`/SingleGame/${game.Id}`);

                var leagues = await axios.get(`/Leagues/`);

                var leagueGame = false;
                var leagueName = '';
                var seasonGame = false;

                for (var league in leagues) {
                    var seasonGames = await axios.get(`/Leagues/${league.SeasonAssignments}/FinalSchedule`);

                    if (seasonGames.data.find(seasonGame => seasonGame.gameId == game.gameId) != null) {
                        leagueGame = true;
                        leagueName = league.LeagueName;
                        seasonGame = true;
                        break;
                    }
                }

                var round = 0;
                var series = false;
                var gameNo = 0;
                var playoffMode = false;

                for (var league in leagues) {
                    var playoffBrackets = await axios.get(`/LeaguePlayoffs/${league.PlayoffAssignments}`);
                    if (playoffBrackets.FinalPlayoffBracket.SubPlayoffBrackets.length == 1) {
                        try {
                            var matchup = await axios.get(`/LeaguePlayoffs/${league.PlayoffAssignments}/${game.hostPlayer}/${game.guestPlayer}/${playoffBrackets.FinalPlayoffBracket.SubPlayoffBrackets[0].playoffName}`);
                        } catch (e) {
                            break;
                        }
                        round = matchup.data.round;
                        if (matchup.data.GameId > 1) {
                            series = true;
                            gameNo = matchup.data.GameId.indexof(game.gameId) + 1;
                        } 
                        playoffMode = true;
                        break;
                    }
                    for (var bracket in playoffBrackets.FinalPlayoffBracket.SubPlayoffBrackets) {
                        if (bracket.PlayoffHeadMatchups.find(matchup => {return matchup.currentPlayoffMatchup.player1 && (matchup.currentPlayoffMatchup.player1 == game.hostPlayer || matchup.currentPlayoffMatchup.player1 == game.guestPlayer)}) != null) {
                            var matchup = await axios.get(`/LeaguePlayoffs/${league.PlayoffAssignments}/${game.hostPlayer}/${game.guestPlayer}/${bracket.playoffName}`);
                            round = matchup.data.round;
                            if (matchup.data.GameId > 1) {
                                series = true;
                                gameNo = matchup.data.GameId.indexof(game.gameId) + 1;
                            } 
                            playoffMode = true;
                        }
                    }
                }

                var gameInfo = {
                    Id : game.Id,
                    Matchup : game.Matchup,
                    GameTime : res.data.timePlayed,
                    hostScore : res.data.hostScore,
                    guestScore : res.data.guestScore,
                    final : res.data.finalScore != null ? true : false,
                    league : leagueGame,
                    leagueName : leagueName,
                    seasonMode: seasonGame,
                    playoffMode : playoffMode,
                    Round : round,
                    Series : series,
                    Game : gameNo
                };

                gameResultsCopy.push(gameInfo);
            }

            setGameResults(gameResultsCopy);

            const leagueRes = await axios.get(`/Search/League/${q}`);

            var leagueResultsCopy = [];

            for (var league in leagueRes.data) {
                var res = await axios.get(`/League/${league.LeagueId}`);

                var assignments = await axios.get(`/LeagueSeasonAssignments/${res.data.SeasonAssignments}`);

                var leagueInfo = {
                    Name: league.LeagueName,
                    Tags: res.data.Tags,
                    Description: res.data.Description,
                    Owner: res.data.Owner,
                    PlayerCount: res.data.Players.length,
                    started: assignments.data.PlayerFullSchedule.length > 0 ? true : false
                };

                leagueResultsCopy.push(leagueInfo);
            }

            setLeagueResults(leagueResultsCopy);

        };

        if (q) {
            setQuery(q);

            fetchAllResults(q);
        }
    }, [location.search]);

    const handleSearchSubmit = (event) => {
        event.preventDefault();

        history.push(`/SearchResults?query=${encodeURIComponent(query)}`);
    };
};

export default SearchResults;