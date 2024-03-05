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

    const playoffScores = async (player) => {
        var leagueInfo = await getPlayerLeague(player);
        const playoffs = await axios.get(`/LeaguePlayoffs/${leagueInfo.playoffAssignments}`);

        var single = true;
        var index = 0;
        var f = false;
        var bracketName = playoffs.FinalFullBracket.SubPlayoffBrackets[0].playoffName;
        if (playoffs.FinalFullBracket.SubPlayoffBrackets.length > 1) {
            single = false;
            var res = isInBracket(player, playoffsId);
            if (res) {
                bracketName = res;
                f = true;
            }
            index++;
        }

        if (!f) {
            return [];
        }

        var trail = await axios.get(`/LeaguePlayoffs/${playoffsId}/${player}/${single}/${bracketName}/PlayoffRunTrail`);

        return trail.data;
    };

    const findSeriesWinner = async (temp, gameId) => {
        var gameInfo = await axios.get(`/SingleGame/${gameId}`);

        var matchups = await playoffScores(gameInfo.hostPlayer);

        for (var matchup in matchups) {
            if (matchup.GameId.find(game => game == gameId) != null) {
                var i = 0;
                while (matchup.GameId[i] != gameId) {
                    var gameEntry = await axios.get(`/SingleGame/${matchup.GameId[i]}`);
                    if (gameEntry.hostScore > gameEntry.guestScore) {
                        temp.hostWins++;
                    } 
                    if (gameEntry.hostScore < gameEntry.guestScore) {
                        temp.guestWins++;
                    }
                    i++;
                }
                if (temp.hostScore > temp.guestScore) {
                    temp.hostWins++;
                }
                if (temp.hostScore < temp.guestScore) {
                    temp.guestWins++;
                }
                break;
            }
        }

        var leagueInfo = await getPlayerLeague(player);

        const configInfo = await axios.get(`/LeagueConfig/${leagueInfo.LeagueConfig}`);

        var game_ct = configInfo.data.gamesByRound[matchup.round-1];

        if (game_ct == temp.hostWins || game_ct == temp.guestWins) {
            temp.seriesWinner = true;
        }
    };

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

                var round = "";
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
                        round = `${matchup.data.round}`;
                        if (matchup.data.GameId > 1) {
                            series = true;
                            gameNo = matchup.data.GameId.indexof(game.gameId) + 1;
                        } 
                        playoffMode = true;
                        leagueGame = true;
                        leagueName = league.leagueName;
                        break;
                    }

                    const groupByFirstElement = (arr) => {
                        return arr.reduce((result, currentArray) => {
                            const key = currentArray[0]; // Get the first element as the key
                            if (!result[key]) {
                                result[key] = []; // If the key doesn't exist in the result, create a new array for it
                            }
                            result[key].push(currentArray); // Push the current array into the corresponding group
                            return result;
                        }, {});
                    };


                    for (var bracket in playoffBrackets.FinalPlayoffBracket.SubPlayoffBrackets) {
                        if (bracket.PlayoffHeadMatchups.find(matchup => {return matchup.currentPlayoffMatchup.player1 && (matchup.currentPlayoffMatchup.player1 == game.hostPlayer || matchup.currentPlayoffMatchup.player1 == game.guestPlayer)}) != null) {
                            var matchup = await axios.get(`/LeaguePlayoffs/${league.PlayoffAssignments}/${game.hostPlayer}/${game.guestPlayer}/${bracket.playoffName}`);
                            var extra = "";
                            var temp = groupByFirstElement(bracket.AllOtherMatchups);
                            if ((temp.length + 1) - matchup.data.round == 1) {
                                extra += " (Semifinals)";
                            }
                            if ((temp.length + 1) == matchup.data.round) {
                                extra += " (Championship)";
                            }
                            round = `${matchup.data.round}${extra}`;
                            if (matchup.data.GameId > 1) {
                                series = true;
                                gameNo = matchup.data.GameId.indexof(game.gameId) + 1;
                            } 
                            playoffMode = true;
                            leagueGame = true;
                            leagueName = league.leagueName;
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

                if (gameInfo.playoffMode && gameInfo.final && gameInfo.Series) {
                    findSeriesWinner(gameInfo, gameInfo.gameId);
                }

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

    useEffect(() => {
        const updateScores = async () => {
            const res = await axios.get(`/data/AddInGameScore/app`);

            const gameId = res.data.substring(0, res.data.indexof("_"));
 
            var parts = res.data.substring(res.data.indexof("_")+1).split(",");
            var scores = parts.map(part => part.trim());

            scores[0] = scores[0];
            scores[1] = [scores[1].match(/\(([^,]+),\s*([^)]+)\)/).slice(1)];

            var gameResultsCopy = gameResults;

            var foundGame = gameResultsCopy.find(game => game.gameId == gameId);
            if (foundGame) {
                foundGame.hostScore = parseInt(scores[1][0]);
                foundGame.guestScore = parseInt(score[1][1]);
            }
            setGameResults(gameResultsCopy);
        };

        updateScores();

    }, []);

    useEffect(() => {
        const updateScores = async () => {
            const res = await axios.get(`/data/UpdateSingleGameFinalScore/app`);

            const gameId = res.data.substring(0, res.data.indexof("_"));
 
            var scores = res.data.substring(res.data.indexof("_")+1).split(",");

            scores[0] = parseInt(scores[0]);
            scores[1] = parseInt(scores[1]);

            var gameResultsCopy = gameResults;

            var foundGame = gameResultsCopy.find(game => game.gameId == gameId);
            if (foundGame) {
                foundGame.hostScore = parseInt(scores[1][0]);
                foundGame.guestScore = parseInt(score[1][1]);
                foundGame.finalScore = scores;
                if (foundGame.playoffMode && gameInfo.final && gameInfo.Series) {
                    findSeriesWinner(foundGame, foundGame.gameId);
                }
            }
            setGameResults(gameResultsCopy);
        };

        updateScores();

    }, []);

    useEffect(() => {
        const changePlayerAvailability = async () => {
            const res = await axios.get(`/data/ChangedAvailableStatus/app`);

            const playerId = res.data.substring(0, res.data.indexof("_"));

            const availability = res.data.substring(res.data.indexof("_")+1);

            var playerResultsCopy = playerResults;

            var foundPlayer = playerResultsCopy.find(player => player.PlayerId == playerId);
            if (foundPlayer) {
                foundPlayer.IsAvailable = availability == "available" ? true : false;
            }

            setPlayerResults(playerResultsCopy);
        };

        changePlayerAvailability();

    }, []);

    useEffect(() => {
        const changePlayerGameStatus = async () => {
            const res = await axios.get(`/data/ChangedGameStatus/app`);

            const playerId = res.data.substring(0, res.data.indexof("_"));

            const gameStatus = res.data.substring(res.data.indexof("_")+1);

            var playerResultsCopy = playerResults;

            var foundPlayer = playerResultsCopy.find(player => player.PlayerId == playerId);
            if (foundPlayer) {
                foundPlayer.GameStatus = gameStatus == "Playing in game" ? true : false;
                if (foundPlayer.GameStatus) {
                    foundPlayer.IsAvailable = false;
                }
            }

            setPlayerResults(playerResultsCopy);
        };

        changePlayerGameStatus();

    }, []);

    const handleSearchSubmit = (event) => {
        event.preventDefault();

        history.push(`/SearchResults?query=${encodeURIComponent(query)}`);
    };
};

export default SearchResults;