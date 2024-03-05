import { useRef, useState, useEffect, useContext } from 'react';
import axios from './api/axios';
import { useHistory } from 'react-router-dom'; 
import {useAuth} from "./context/AuthProvider";

const LeaguePortal = (leagueId) => {
    const [loggedIn] = useAuth();

    const [tags, setTags] = useState([]);

    const [description, setDescription] = useState('');

    const [players, setPlayers] = useState([]);

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

    const [combinedDivisionMode, setCombinedDivisonMode] = useState(false);

    const [wholeModeOrdering, setWholeModeOrdering] = useState([]);

    const [divisonModeOrdering, setDivisionModeOrdering] = useState([]);

    const [combinedDivisionOrdering, setCombinedDivisionModeOrdering] = useState([]);

    const [userDefinedOrdering, setUserDefinedOrdering] = useState([]);

    const [finalPlayoffBracket, setFinalPlayoffBracket] = useState(false);

    const [archievePlayoffBrackets, setArchievePlayoffBrackets] = useState([]);

    useEffect(() => {

    }, []);

};

export default LeaguePortal;