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

    const [playerSchedules, setPlayerSchedules] = useState(null);

    const [fullSchedule, setFullSchedule] = useState(null);

    const [playoffsMode, setPlayoffsMode] = useState(false);

    const [wholeMode, setWholeMode] = useState(false);

    const [wholeModeOrdering, setWholeModeOrdering] = useState([]);

    const [divisonModeOrdering, setDivisionModeOrdering] = useState([]);

    const [combinedDivisionOrdering, setCombinedDivisionModeOrdering] = useState([]);

    const [userDefinedOrdering, setUserDefinedOrdering] = useState([]);

    const [finalPlayoffBracket, setFinalPlayoffBracket] = useState(false);

    const [archievePlayoffBrackets, setArchievePlayoffBrackets] = useState([]);

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


        };

        fetchData();
    }, []);

};

export default LeaguePortal;