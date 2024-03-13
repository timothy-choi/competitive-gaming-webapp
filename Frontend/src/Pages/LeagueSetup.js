import { useRef, useState, useEffect, useContext } from 'react';
import axios from './api/axios';
import { useHistory } from 'react-router-dom'; 
import {useAuth} from "./context/AuthProvider";

const LeagueSetup = (username) => {
    const [loggedIn, username] = useAuth();

    const history = useHistory();

    const [errorMessage, setErrorMessage] = useState('');

    const handleSubmit = (event) => {
        event.preventDefault();

        var LeagueCreationInput = {
            Name: event.target.name.value,
            Description: event.target.name.value,
            Owner: username,
            StartDate: event.target.startDate.value
        };

        const processLeagueCreationInput = async (input) => {
            try {
                const check = await axios.get(`/Leagues`);

                for (var elt in check.data) {
                    if (elt.Name == event.target.name.value) {
                        setErrorMessage('League name already taken');
                        return;
                    }
                }
                const res = await axios.post(`/Leagues`, input);

                return res.data;

            } catch (e) {
                setErrorMessage('Could not create league');
                return;
            }
        };

        if (event.target.NumberOfPlayersMin.value < 3) {
            setErrorMessage('Too few games');
            return;
        }

        if (event.target.NumberOfPlayersMin.value < 4 || event.target.NumberOfPlayersLimit.value < 4) {
            setErrorMessage('Too few games');
            return;
        }

        if (event.target.intervalBetweenGames.value < 1 || event.target.playoffStartOffset.value < 1 || event.target.intervalBetweenPlayoffRoundGames.value < 1 || event.target.intervalBetweenRounds.value < 1) {
            setErrorMessage('Invalid number of days');
            return;
        }

        if (event.target.playoffEligibleLimit.value && event.target.playoffSizeLimit.value >= event.target.NumberOfPlayersLimit.value) {
            setErrorMessage('Invalid number of players allowed in playoffs');
            return;
        }

        if (!event.target.playoffEligibleLimit.value && event.target.playoffSizeLimit.value > event.target.NumberOfPlayersLimit.value) {
            setErrorMessage('Invalid number of players allowed in playoffs');
            return;
        }

        if (event.target.PlayoffSeries.value && event.target.SeriesLengthMax.value <= 1 || !event.target.PlayoffSeries.value && event.target.SeriesLengthMax.value == 0) {
            setErrorMessage(`Invalid number of games`);
            return;
        }

        if (!event.target.PlayoffSeries.value && event.target.SeriesLengthMax.value > 1) {
            setErrorMessage(`Invalid number of games`);
            return;
        }

        if (event.target.GamesPerRound.length == 0) {
            setErrorMessage(`Invalid GamesPerRound Input`);
            return;
        }

        var league = processLeagueCreationInput(LeagueCreationInput);

        var configInput = {
            LeagueName : league.Name,
            commitmentLength : event.target.commitmentLength.value,
            feePrice : event.target.feePrice.value,
            NumberOfPlayersLimit : event.target.NumberOfPlayersLimit.value,
            OwnerAsPlayer : event.target.OwnerAsPlayer.value,
            NumberOfPlayersMin : event.target.NumberOfPlayersMin.value,
            NumberOfGames : event.target.NumberOfPlayersMin.value,
            selfScheduleGames : event.target.selfScheduleGames.value,
            intervalBetweenGames : event.target.intervalBetweenGames.value,
            intervalBetweenGamesHours : event.target.intervalBetweenGameHours.value,
            playoffStartOffset : event.target.playoffStartOffset.value,
            intervalBetweenPlayoffRoundGames : event.target.intervalBetweenPlayoffRoundGames.value,
            intervalBetweenPlayoffRoundGamesHours : event.target.intervalBetweenPlayoffRoundGamesHours.value,
            intervalBetweenRounds : event.target.intervalBetweenRounds.value,
            intervalBetweenRoundsHours : event.target.intervalBetweenRoundsHours.value,
            playoffContention : event.target.playoffContention.value,
            playoffEligibleLimit : event.target.playoffEligibleLimit.value,
            PlayoffSizeLimit : event.target.playoffSizeLimit.value,
            PlayoffSeries : event.target.PlayoffSeries.value,
            SeriesLengthMax : event.target.SeriesLengthMax.value,
            sameSeriesLength : event.target.sameSeriesLength.value,
            GamesPerRound : event.target.GamesPerRound.value,
            otherMetrics : event.target.otherMetrics.value
        };


        const processConfigInput = async (configInput) => {
            try {
                const res = await axios.post("/LeagueConfig/", configInput);

                return res.data;
            } catch (e) {
                setErrorMessage(`Failed to process config info`);
                return;
            }
        };

        const configId = processConfigInput(configInput);

        if (event.target.PartitionsEnabled.value) {
            if (event.target.NumberOfPlayersPerPartition.value <= 1) {
                setErrorMessage(`Invalid number of players per partition`);
                return;
            }
            
            if (event.target.NumberOfPartitions.value <= 1) {
                setErrorMessage(`Invalid number of partitions`);
                return;
            }
        }

        if (event.target.ExcludeOutsideGames.value && event.target.InterDvisionGameLimit.value > 0) {
            setErrorMessage('Invalid number of outside games');
            return;
        }

        if (!event.target.RepeatMatchups.value && event.target.MaxRepeatMatchups.value > 0) {
            setErrorMessage('Invalid number of repeat matchup games');
            return;
        }

        if (event.target.repeatAllMatchups.value && event.target.minRepeatMatchups.value < 1) {
            setErrorMessage('Invalid number of repeat matchup games');
            return;
        }

        if (event.target.PartitionsEnabled.value && (event.target.NumberOfPlayersPerPartition.value * event.target.NumberOfPartitions.value) != configInput.NumberOfPlayersMin) {
            setErrorMessage(`Invalid input for either Number of Players per partition and/or number of partitions. The number of partitions times the number of players per partition must be equal to the number of required players in a league.`);
            return;
        }


        const seasonAssignmentInfo = {
            ConfigId : configId,
            LeagueId : league.LeagueId,
            PartitionsEnabled : event.target.PartitionsEnabled.value,
            ReassignEverySeason : event.target.ReassignEverySeason.value,
            AutomaticInduction : event.target.AutomaticInduction.value,
            NumberOfPlayersPerPartition : event.target.NumberOfPlayersPerPartition.value,
            NumberOfPartitions : event.target.NumberOfPartitions.value,
            SamePartitionSize : event.target.SamePartitionSize.value,
            AutomaticScheduling : event.target.AutomaticScheduling.value,
            ExcludeOutsideGames : event.target.ExcludeOutsideGames.value,
            InterDvisionGameLimit : event.target.InterDvisionGameLimit.value,
            RepeatMatchups : event.target.RepeatMatchups.value,
            MaxRepeatMatchups : event.target.MaxRepeatMatchups.value,
            DivisionSelective : event.target.DivisionSelective.value,
            RandomizeDivisionSelections : event.target.RandomizeDivisionSelections.value,
            PlayerSelection : event.target.PlayerSelection.value,
            repeatAllMatchups : event.target.repeatAllMatchups.value,
            minRepeatMatchups : event.target.minRepeatMatchups.value,
            maxRepeatMatchups : event.target.maxRepeatMatchups.value,
            playAllPlayers : event.target.playAllPlayers.value
        };

        const processSeasonAssignments = async (assignmentInfo) => {
            try {
                const res = await axios.post(`/LeagueSeasonAssignments`, assignmentInfo);

                return res.data;
            } catch (e) {
                setErrorMessage(`Failed to process season assignments info`);
                return;
            }
        }

        var seasonAssignmentsId = processSeasonAssignments(seasonAssignmentInfo);

        var playoffId = "";


        if (configInput.playoffContention) {
            const playoffInfo = {
                LeagueId : league.LeagueId,
                RandomInitialMode : event.target.RandomInitialMode.value,
                RandomRoundMode : event.target.RandomRoundMode.value,
                WholeMode : event.target.WholeMode.value,
                DefaultMode : event.target.DefaultMode.value,
                CombinedDivisionMode : event.target.CombinedDivisionModeMode.value,
                DivisionMode : event.target.DivisionMode.value,
                PlayoffNames : event.target.PlayoffNames.value.split(",").map(item=>item.trim())
            };
    
            const processPlayoffs = async (playoffInfo) => {
                try {
                    const res = await axios.post(`/LeaguePlayoffs`, playoffInfo);
    
                    return res.data;
                } catch (e) {
                    setErrorMessage(`Failed to process playoffs info`);
                    return;
                }
            }

            playoffId = processPlayoffs(playoffInfo);
        }

        var addIds = async (configId, seasonAssignmentsId, playoffsId) => {
            try {
                const res = await axios.put(`/League/${league.LeagueId}/${configId}`);

                res = await axios.put(`/League/${league.LeagueId}/${seasonAssignmentsId}`);

                if (playoffId != "") {
                    res = await axios.put(`/League/${league.LeagueId}/${playoffsId}`);
                }
            } catch (e) {
                setErrorMessage(`Failed to process ids`);
                return;
            }
        }

        addIds(configId, seasonAssignmentsId, playoffId);
    };

    const handleFinishSubmit = (event) => {
        event.preventDefault();

        history.push(`/League/${league.LeagueId}`);
    }
};

export default LeagueSetup;