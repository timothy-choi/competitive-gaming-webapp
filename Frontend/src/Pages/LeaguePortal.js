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
                    username : player[0],
                    playerId : res.data.playerId,
                    dateJoined : player[1]
                };

                players.push(playerInfo);
            }

            setPlayers(players);

            setLeagueConfig(leagueInfo.data.LeagueConfig);

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
                        continue;
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
                        continue;
                    }
                    allGames.push(curr);
                }
                setSeasonGamesByDate(allGames);
            }
        };

        fetchData();
    }, []);

};

export default LeaguePortal;