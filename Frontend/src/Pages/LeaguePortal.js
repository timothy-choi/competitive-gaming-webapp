import { useRef, useState, useEffect, useContext } from 'react';
import axios from './api/axios';
import { useHistory } from 'react-router-dom'; 
import {useAuth} from "./context/AuthProvider";

const LeaguePortal = (leagueId) => {
    const [loggedIn, username] = useAuth();

    const [name, setName] = useState('');

    const [tags, setTags] = useState([]);

    const [description, setDescription] = useState('');

    const [players, setPlayers] = useState([]);

    const [owner, setOwner] = useState([]);

    const [leagueHoldStatus, setLeagueHoldStatus] = useState(false);

    const [playoffsStart, setPlayoffsStart] = useState(false);

    const [leagueStandings, setLeagueStandings] = useState(null);

    const [leagueConfig, setLeagueConfig] = useState('');

    const [seasonAssignments, setSeasonAssignments] = useState('');

    const [playoffAssignemnts, setPlayoffAssignments] = useState('');

    const [archieveLeagueStandings, setArchieveLeagueStandings] = useState([]);

    const [divisionMode, setDivisionMode] = useState(false);

    const [divisionStandings, setDivisionStandings] = useState(null);

    const [archieveDivisionStandings, setArchieveDivisionStandings] = useState([]);

    const [combinedDivisionsMode, setCombinedDivisionsMode] = useState(false);

    const [combinedDivisionsStandings, setCombinedDivisionsStandings] = useState(null);

    const [archieveCombinedDivisionsStandings, setArchieveCombinedDivisionsStandings] = useState([]);

    const [champions, setChampions] = useState([]);

    const [playerSchedules, setPlayerSchedules] = useState([]);

    const [archievePlayerSchedules, setArchievePlayerSchedules] = useState([]);

    const [fullSchedule, setFullSchedule] = useState([]);

    const [archieveFullSchedule, setArchieveFullSchedule] = useState([]);

    const [playoffsMode, setPlayoffsMode] = useState(false);

    const [wholeMode, setWholeMode] = useState(false);

    const [wholeModeOrdering, setWholeModeOrdering] = useState([]);

    const [divisonModeOrdering, setDivisionModeOrdering] = useState([]);

    const [combinedDivisionOrdering, setCombinedDivisionModeOrdering] = useState([]);

    const [userDefinedOrdering, setUserDefinedOrdering] = useState([]);

    const [finalPlayoffBracket, setFinalPlayoffBracket] = useState(false);

    const [archievePlayoffBrackets, setArchievePlayoffBrackets] = useState([]);

    const [seasonGamesByDate, setSeasonGamesByDate] = useState([]);

    const [currentUserSeasonGame, setCurrentUserSeasonGame] = useState(null);

    const [currentPlayoffGames, setCurrentPlayoffGames] = useState([]);

    const [currentUserPlayoffGame, setCurrentUserPlayoffGame] = useState(null);

    const [roundMatchups, setRoundMatchups] = useState([]);
 
    useEffect(() => {
        const fetchData = async () => {
            const leagueInfo = await axios.get(`/League/${LeagueId}`);

            setName(leagueInfo.data.Name);

            setDescription(leagueInfo.data.Description);

            setOwner(leagueInfo.data.Owner);

            setTags(leagueInfo.data.Tags);

            var players = [];

            for (var player in leagueInfo.data.Players) {
                var res = await axios.get(`/Player/${player[0]}`);
                var playerInfo = {
                    username : player["playerName"],
                    playerId : res.data.playerId,
                    dateJoined : player["dateJoined"]
                };

                players.push(playerInfo);
            }

            setPlayers(players);

            setLeagueConfig(leagueInfo.data.LeagueConfig);

            var configData = await axios.get(`/LeagueConfig/${leagueInfo.data.LeagueConfig}`);

            setLeagueHoldStatus(players.length < configData.data.NumberOfPlayersMin);

            setPlayoffAssignments(leagueInfo.data.PlayoffAssignments);

            setSeasonAssignments(leagueInfo.data.SeasonAssignments);

            var champs = [];

            var index = 0;

            for (var champ in leagueInfo.data.Champions) {
                var champInfo = {
                    username: champ[0],
                    playerId: champ[1],
                    season: index + 1
                }

                champs.push(champInfo);

                index++;
            }

            setChampions(champs);

            setLeagueStandings(leagueInfo.data.LeagueStandings);

            setArchieveLeagueStandings(leagueInfo.data.archieveLeagueStandings);

            const seasonInfo = await axios.get(`/LeagueSeasonAssignments/${seasonAssignments}`);

            if (seasonInfo.data.partitionsEnabled) {
                setDivisionMode(true);
                setDivisionStandings(leagueInfo.data.DivisionStandings);
                setArchieveDivisionStandings(leagueInfo.data.archieveDivisionStandings);
                if (Object.keys(seasonInfo.data.AllCombinedDivisions).length > 0) {
                    setCombinedDivisionsMode(true);
                    setCombinedDivisionsStandings(leagueInfo.data.combinedDivisionsStandings);
                    setArchieveCombinedDivisionsStandings(leagueInfo.data.archieveCombinedDivisionsStandings);
                }
            }

            setPlayerSchedules(leagueInfo.data.PlayerFullSchedule);

            setArchievePlayerSchedules(leagueInfo.data.ArchievePlayerFullSchedule);

            setFullSchedule(leagueInfo.data.FinalFullSchedule);

            setArchieveFullSchedule(leagueInfo.data.ArchieveFinalFullSchedule);

            if (leagueInfo.data.PlayoffAssignments) {
                setPlayoffsMode(true);
                var playoffsInfo = await axios.get(`/LeaguePlayoffs/${leagueInfo.data.PlayoffAssignments}`);
                setWholeMode(playoffsInfo.data.wholeMode);
                if (wholeMode) {
                    setWholeModeOrdering(playoffsInfo.data.wholeModeOrdering);
                }
                if (seasonInfo.data.partitionsEnabled) {
                    setDivisionModeOrdering(playoffsInfo.data.DivisionBasedPlayoffPairings);
                    if (Object.keys(seasonInfo.data.AllCombinedDivisions).length > 0) {
                        setCombinedDivisionModeOrdering(playoffsInfo.data.CombinedDivisionGroups);
                    }
                    if (playoffsInfo.data.UserDefinedPlayoffMatchups.length > 0) {
                        setUserDefinedOrdering(playoffsInfo.data.UserDefinedPlayoffMatchups);
                    }
                }
                setFinalPlayoffBracket(playoffsInfo.data.FinalPlayoffBracket);
                setArchievePlayoffBrackets(playoffsInfo.data.ArchievePlayoffBrackets);
            }

            const getPrevGames = async (url) => {
                try {
                    const response = await axios.get(url);
                    if (response.status === 200) {
                        return response.data;
                    } else {
                        return;
                    }
                } catch (error) {
                    if (error.response && error.response.status === 404) {
                        return; 
                    }
                }
            };

            const getGameInfo = async (gameId, leagueId) => {
                const res = await axios.get(`/SingleGame/${gameId}`);

                const hostInfo = await axios.get(`/Player/${res.data.hostPlayer}`);

                const hostRecord = await axios.get(`/League/${leagueId}/${hostInfo.data.playerId}`);

                const guestInfo = await axios.get(`/Player/${res.data.guestPlayer}`);

                const guestRecord = await axios.get(`/League/${leagueId}/${guestInfo.data.playerId}`);

                var gameInfo = {
                    gameId : gameId,
                    hostPlayer : res.data.hostPlayer,
                    guestPlayer : res.data.guestPlayer,
                    hostScore : res.data.hostScore,
                    guestScore : res.data.guestScore,
                    final : res.data.finalScore != null ? true : false,
                    timePlayed : res.data.timePlayed <= Date.now() ? null : res.data.timePlayed,
                    hostRecord : [hostRecord.data["wins"], hostRecord.data["losses"], hostRecord.data["draws"]],
                    guestRecord : [guestRecord.data["wins"], guestRecord.data["losses"], guestRecord.data["draws"]]
                };

                return gameInfo;
            };

            try {
                var date = new Date();
                const games = await axios.get(`/LeagueSeasonAssignments/${leagueInfo.data.SeasonAssignments}/GamesByDate/${date.getFullYear()}-${date.getMonth()}-${date.getDate()}`);
                var allGames = [];
                for (var game in games.data) {
                    var curr = getGameInfo(game, leagueInfo.data.LeagueId);
                    if (curr.hostPlayer == username || curr.guestPlayer == username) {
                        setCurrentUserSeasonGame(curr);
                    }
                    allGames.push(curr);
                }
                setSeasonGamesByDate(allGames);
            } catch (e) {
                date.setDate(date.getDate()-1);
                const lastSet = null; 
                while (lastSet == null) {
                    date.setDate(date.getDate()-1);
                    lastSet = getPrevGames(`/LeagueSeasonAssignments/${leagueInfo.data.SeasonAssignments}/GamesByDate/${date.getFullYear()}-${date.getMonth()}-${date.getDate()}`);
                }
                var allGames = [];
                for (var game in lastSet) {
                    var curr = getGameInfo(game, leagueInfo.data.LeagueId);
                    if (curr.hostPlayer == username || curr.guestPlayer == username) {
                        setCurrentUserSeasonGame(curr);
                    }
                    allGames.push(curr);
                }
                setSeasonGamesByDate(allGames);
            }

            var playoffInfo = await axios.get(`/LeaguePlayoffs/${playoffAssignemnts}`);

            const getPlayerCurrentPlayoffGame = async (leagueId, bracket, playoffId, player, single) => {
                const res = await axios.get(`/LeaguePlayoffs/${playoffId}/${player}/${single}/${bracket}/PlayoffRunTrail`);

                var lastMatchup = res.data[res.data.length-1];

                var currGame = await axios.get(`/SingleGame/${lastMatchup.GameId[lastMatchup.GameId.length-1]}`);

                var leagueInfo = await axios.get(`/League/${leagueId}`);

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

                var temp = groupByFirstElement(leagueInfo.FinalPlayoffBracket.SubPlayoffBrackets.find(b => b.playoffName == bracket).AllOtherMatchups);

                var round = "";

                var trail_length = res.data.length;
                if (single) {
                    if (trail_length <= temp.length + 1) {
                        round = ` Round ${trail_length}`;
                    }
                    else {
                        var num_rds = leagueInfo.FinalPlayoffBracket.SubPlayoffBrackets.length; 
                        var ct = 0;
                        while (num_rds != 1) {
                            num_rds /= 2;
                            ct++;
                        }
                        if ((temp.length + ct + 1) - trail_length == 1) {
                            round = `Semifinals`;
                        }
                        else if ((temp.length + ct + 1) == trail_length) {
                            round = `Championship`;
                        }
                        else {
                            round = `Final Round ${trail_length - temp.length + 1}`;
                        }
                    }
                }
                else {
                    if (temp.length + 1 - trail_length == 1) {
                        round = `Round ${trail_length} (Semifinals)`;
                    }
                    if (temp.length + 1 == trail_length) {
                        round = `Round ${trail_length} (Championship)`;
                    }
                    else {
                        round = `Round ${trail_length}`;
                    }
                }

                var configInfo = await axios.get(`LeagueConfig/${leagueInfo.LeagueConfig}`);



                var gameInfo = {
                    gameId : currGame.data.gameId,
                    hostPlayer : currGame.data.hostPlayer,
                    guestPlayer : currGame.data.guestPlayer,
                    hostScore : currGame.data.hostScore,
                    guestScore : currGame.data.guestScore,
                    final : currGame.data.finalScore != null ? true : false,
                    timePlayed : currGame.data.timePlayed > Date.now() ? null : currGame.data.timePlayed,
                    round: round,
                    series: configInfo.data.PlayoffSeries,
                    game: lastMatchup.GameId.length,
                    host_wins : lastMatchup.player1 == hostPlayer ? lastMatchup.series_player1_wins : lastMatchup.series_player2_wins,
                    guest_wins : lastMatchup.player1 == guestPlayer ? lastMatchup.series_player1_wins : lastMatchup.series_player2_wins,
                    winner : configInfo.data.GamesPerRound[trail_length-1] == host_wins || configInfo.data.GamesPerRound[trail_length-1] == guest_wins ? true : false
                };

                return gameInfo;
            };

            var seen = {};

            var latestPlayoffGames = [];

            for (var player in players) {
                if (seen[player.username]) {
                    continue;
                }
                var found = false;
                var bracketName = "";
                for (var bracket in playoffInfo.data.FinalPlayoffBracket.SubPlayoffBracket) {
                    if (bracket.PlayoffHeadMatchups.find(matchup => matchup.currentPlayoffMatchup.player1 == player.username || matchup.currentPlayoffMatchup.player2 == player.username) != null) {
                        found = true;
                        bracketName = bracket.playoffName;
                        break;
                    }
                }
                if (found) {
                    latestPlayoffGames.push(getPlayerCurrentPlayoffGame(leagueId, bracketName, playoffAssignemnts, player.username, playoffInfo.data.FinalPlayoffBracket.SubPlayoffBracket > 1));
                    seen[player.username] = true;
                }
            }

            setCurrentPlayoffGames(latestPlayoffGames);

            for (var game in latestPlayoffGames) {
                if (game.hostPlayer == username || game.guestPlayer == username) {
                    setCurrentUserPlayoffGame(game);
                    break;
                }
            }


            const getGamesByRound = async (round, division, playoffsId) => {
                const res = await axios.get(`/LeaguePlayoffs/${playoffsId}/${division}/GamesPerRound/${round}`);

                const allMatches = [];

                for (var matchup in res.data["roundGames"]) {
                    if (matchup.player1 == "" || matchup.player2 == "") {
                        return [];
                    }
                    allMatches.push(matchup);
                }

                return allMatches;
            };

            var single = true;
            if (playoffInfo.data.FinalPlayoffBracket.SubPlayoffBrackets.length > 1) {
                single = false;
            }

            var recentRoundMatchups = [];

            for (var bracket in playoffInfo.data.FinalPlayoffBracket.SubPlayoffBrackets) {
                var temp = groupByFirstElement(leagueInfo.FinalPlayoffBracket.SubPlayoffBrackets.find(b => b.playoffName == bracket).AllOtherMatchups);

                var bracket_len = temp.length + 1;

                var error = false;

                for (var i = 0; i < bracket_len; ++i) {
                    var matches = getGamesByRound(i+1, bracket.playoffName, playoffAssignemnts);
                    if (matches.length == 0) {
                        error = true;
                        break;
                    }

                    var rd = "";

                    if (single) {
                        if (i == bracket_len) {
                            rd = `Round ${i} (Championship)`;
                        }
                        else if (bracket_len - i == 1) {
                            rd = `Round ${i} (Semifinals)`;
                        }
                        else {
                            rd = `Round ${i}`;
                        }
                    } else {
                        var num_brackets = leagueInfo.FinalPlayoffBracket.SubPlayoffBrackets.length;
                        var add_rds = sqrt(num_brackets);
                        if (bracket_len + add_rds == i) {
                            rd = `Championship`;
                        }
                        if (bracket_len + add_rds - i == 1) {
                            rd = `Semifinal`;
                        }
                        else {
                            rd = `Final Round ${i - bracket_len}`;
                        }
                    }

                    recentRoundMatchups.push([bracket.playoffName, rd, matches]); 
                }

                if (error) {
                    break;
                }
            }

            setRoundMatchups(recentRoundMatchups);

        };

        fetchData();
    }, []);

    useEffect(() => {
        const AddNewPlayer = async () => {
            const res = await axios.get(`/data/AddingNewPlayerInLeague/${leagueId}`);

            const uname = res.data.substring(0, res.data.indexof("_"));

            const date = res.data.substring(res.data.indexof("_")+1);

            const playerInfo = await axios.get(`/Player/${uname}`);

            const newPlayer = {
                username : uname,
                playerId : playerInfo.data.playerId,
                dateJoined : new Date(date)
            };

            var playerCopy = players;

            playerCopy.push(newPlayer);

            setPlayers(playerCopy);
        };

        AddNewPlayer();
    }, []);

    useEffect(() => {
        const RemovePlayer = async () => {
            const res = await axios.get(`/data/RemovingPlayerInLeague/${leagueId}`);

            var playerCopy = players;

            playerCopy.splice(playerCopy.indexof(playerCopy.find(player => player.playerId == res.data)), 1);

            setPlayers(playerCopy);
        };

        RemovePlayer();
    }, []);

    useEffect(() => {
        const AddChampion = async () => {
            const res = await axios.get(`/data/AddNewChampion/${leagueId}`);

            const playerInfo = await axios.get(`/Player/${res.data}`);

            const newChamp = {
                username : res.data,
                playerId : playerInfo.data.playerId,
                season : champions.length + 1
            };

            var championsCopy = champions;

            championsCopy.push(newChamp);

            setChampions(championsCopy);
        }

        AddChampion();
    }, []);

    useEffect(() => {
        const AddPlayerToLeagueStandings = async () => {
            const res = await axios.get(`/data/AddPlayerToLeague/${leagueId}`);

            var leagueStandingsCopy = leagueStandings;

            leagueStandingsCopy.Table.push(JSON.parse(res.data));

            setLeagueStandings(leagueStandingsCopy);
        };

        AddPlayerToLeagueStandings();
    }, []);

    useEffect(() => {
        const RemovePlayerFromLeagueStandings = async () => {
            const res = await axios.get(`/data/RemovePlayerToLeague/${leagueId}`);

            var leagueStandingsCopy = leagueStandings;

            leagueStandingsCopy.Table.splice(leagueStandingsCopy.Table.indexof(leagueStandingsCopy.Table.find(player => player["player"] == res.data)), 1);

            setLeagueStandings(leagueStandingsCopy);
        };

        RemovePlayerFromLeagueStandings();
    }, []);

    useEffect(() => {
        const ResetLeagueStandings = async () => {
            const res = await axios.get(`/data/ResetLeagueStandings/${leagueId}`);

            var leagueStandingsCopy = leagueStandings;

            leagueStandingsCopy = JSON.parse(res.data)["LeagueStandings"];

            setLeagueStandings(leagueStandingsCopy);
        };

        ResetLeagueStandings();
    }, []);

    useEffect(() => {
        const AddPlayerToDivisionStandings = async () => {
            const res = await axios.get(`/data/AddPlayerToDivison/${leagueId}`);

            var divisionStandingsCopy = divisionStandings;

            divisionStandingsCopy.Table.push(JSON.parse(res.data));

            setDivisionStandings(divisionStandingsCopy);
        };

        AddPlayerToDivisionStandings();
    }, []);

    useEffect(() => {
        const RemovePlayerFromDivisionStandings = async () => {
            const res = await axios.get(`/data/DeletePlayerFromDivisionStandings/${leagueId}`);

            var divisionStandingsCopy = divisionStandings;

            divisionStandingsCopy.Table = JSON.parse(res.data);

            setDivisionStandings(divisionStandingsCopy);
        };

        RemovePlayerFromDivisionStandings();
    }, []);

    useEffect(() => {
        const ResetLeagueStandings = async () => {
            const res = await axios.get(`/data/ResetDivisions/${leagueId}`);

            var divisionStandingsCopy = divisionStandings;

            divisionStandingsCopy = JSON.parse(res.data);

            setDivisionStandings(divisionStandingsCopy);
        };

        ResetLeagueStandings();
    }, []);

    useEffect(() => {
        const AddPlayerToCombinedDivisionStandings = async () => {
            const res = await axios.get(`/data/AddPlayerToCombinedStandings/${leagueId}`);

            var combinedDivisionStandingsCopy = combinedDivisionsStandings;

            combinedDivisionStandingsCopy.Table.push(JSON.parse(res.data));

            setCombinedDivisionsStandings(combinedDivisionStandingsCopy);
        };

        AddPlayerToCombinedDivisionStandings();
    }, []);

    useEffect(() => {
        const RemovePlayerFromCombinedDivisionStandings = async () => {
            const res = await axios.get(`/data/RemovePlayerFromCombinedDivision/${leagueId}`);

            var divisionStandingsCopy = combinedDivisionStandings;

            divisionStandingsCopy.Table = JSON.parse(res.data);

            setCombinedDivisionsStandings(divisionStandingsCopy);
        };

        RemovePlayerFromCombinedDivisionStandings();
    }, []);

    useEffect(() => {
        const ResetLeagueCombinedStandings = async () => {
            const res = await axios.get(`/data/ResetCombinedDivisions/${leagueId}`);

            var combinedDivisionStandingsCopy = combinedDivisionStandings;

            combinedDivisionStandingsCopy = JSON.parse(res.data);

            setCombinedDivisionsStandings(divisionStandingsCopy);
        };

        ResetLeagueCombinedStandings();
    }, []);

    useEffect(() => {
        var updateStandings = async () => {
            const res = await axios.get(`/data/UpdateStandings/${leagueId}`);

            var leagueStandingsCopy = leagueStandings;

            var divisionStandingsCopy = divisionStandings;

            var combinedCopy = combinedDivisionsStandings;

            if (res.data.contains(",")) {
                var allStandings = res.data.split(",");

                var leagueTable = JSON.parse(allStandings[0]);

                leagueStandingsCopy.Table = leagueTable;

                setLeagueStandings(leagueStandingsCopy);

                var divisionTable = JSON.parse(allStandings[1]);

                divisionStandingsCopy.Table = divisionTable;

                setDivisionStandings(divsionStandingsCopy);

                if (allStandings.length == 3) {
                    var combinedTable = JSON.parse(allStandings[2]);

                    combinedCopy.Table = combinedTable;

                    setCombinedDivisionsStandings(combinedCopy);
                }
            }
            else {
                var table = JSON.parse(res.data);

                leagueStandingsCopy.Table = table;

                setLeagueStandings(leagueStandingsCopy);
            }
        };

        updateStandings();

    }, []);

    useEffect(() => {
        const updateSeasonInGameScores = async () => {
            var res = await axios.get(`/data/AddInGameScore/app`);

            var scoreInfo = res.data.substring(0, res.data.indexof("_")).split(", ");

            var scores = scoreInfo[1].slice(1, -1).split(", ").map(Number);

            var gameId = res.data.substring(res.data.indexof("_")+1);

            var seasonGamesCopy = seasonGamesByDate;

            var foundGame = seasonGamesCopy.find(game => game.gameId == gameId);

            if (foundGame != null) {
                foundGame.hostScore  = scores[0];
                foundGame.guestScore = scores[1];
                setSeasonGamesByDate(seasonGamesCopy);
            }

            var userGame = currentUserSeasonGame;

            if (userGame.gameId == gameId) {
                userGame.hostScore = scores[0];
                userGame.guestScore = scores[1];
                setCurrentUserSeasonGame(userGame);
            }
        };

        updateSeasonInGameScores();
    }, []);

    useEffect(() => {
        const updateSeasonFinalScores = async () => {
            var res = await axios.get(`/data/UpdateSingleGameFinalScore/app`);

            var scores = res.data.substring(0, res.data.indexof("_")).split(",");

            var gameId = res.data.substring(res.data.indexof("_")+1);

            var seasonGamesCopy = seasonGamesByDate;

            var foundGame = seasonGamesCopy.find(game => game.gameId == gameId);

            if (foundGame != null) {
                foundGame.hostScore  = scores[0];
                foundGame.guestScore = scores[1];
                foundGame.final = true;
                if (scores[0] < scores[1]) {
                    foundGame.hostRecord[1]++;
                    foundGame.guestRecord[0]++;
                }
                else if (scores[0] > scores[1]) {
                    foundGame.hostRecord[0]++;
                    foundGame.guestRecord[1]++;
                }
                else {
                    foundGame.hostRecord[2]++;
                    foundGame.guestRecord[2]++;
                }
                setSeasonGamesByDate(seasonGamesCopy);
            }

            var userGame = currentUserSeasonGame;

            if (userGame.gameId == gameId) {
                userGame.hostScore = scores[0];
                userGame.guestScore = scores[1];
                userGame.final = true;
                if (scores[0] < scores[1]) {
                    userGame.hostRecord[1]++;
                    userGame.guestRecord[0]++;
                }
                else if (scores[0] > scores[1]) {
                    userGame.hostRecord[0]++;
                    userGame.guestRecord[1]++;
                }
                else {
                    userGame.hostRecord[2]++;
                    userGame.guestRecord[2]++;
                }
                setCurrentUserSeasonGame(userGame);
            }
        };

        updateSeasonFinalScores();

    }, []);
};

export default LeaguePortal;