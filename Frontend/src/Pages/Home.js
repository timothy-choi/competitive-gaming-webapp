import { React, useRef, useState, useEffect, useContext } from 'react';
import axios from './api/axios';
import { useHistory } from 'react-router-dom'; 
import {useAuth} from "./context/AuthProvider";


const Home = () => {
    const {loggedIn, username } = useAuth();

    const [friendsGames, setFriendsGames] = useState([]);

    const [currentGame, setCurrentGame] = useState(null);

    const [currentSeasonUpcomingGame, setCurrentSeasonUpcomingGame] = useState(null);

    const [currentSeasonOtherGames, setCurrentSeasonGames] = useState([]);

    const [currentPlayoffGame, setPlayoffUpcomingGame] = useState(null);

    const [currentPlayoffOtherGames, setCurrentPlayoffGames] = useState([]);

    const [currentFriendSeasonGames, setCurrentFriendSeasonGames] = useState([]);

    const [currentFriendPlayoffGames, setCurrentFriendPlayoffGames] = useState([]);

    const [query, setQuery] = useState('');

    const history = useHistory();

    useEffect(() => {
        const getPlayerLeague = async (player) => {
            const leagues = await axios.get(`/League/`);
            const playerLeague = await axios.get(`/Player/${player}`);

            for (var league in leagues.data) {
                if (league.Name == playerLeague.leagueName) {
                    return league;
                }
            }

            return null;
        };


        const getSeasonGamesInLeague = async (player, assignmentsId) => {
            const res = await axio.get(`/LeagueSeasonAssignments/${assignmentsId}/${player}/PlayerSchedule`);

            return res.data["schedule"];
        };

        const isInBracket = async (player, playoffsId) => {
            const playoffs = await axios.get(`/LeaguePlayoffs/${playoffsId}`);

            for (var bracket in playoffs.FinalFullBracket.SubPlayoffBrackets) {
                for (var head in bracket.PlayoffHeadMatchups) {
                    if (head.Player1 == player || head.Player2 == player) {
                        return bracket.playoffName;
                    }
                }
            }

            return null;
        }

        const getPlayoffGamesInLeague = async (player, playoffsId) => {
            const playoffs = await axios.get(`/LeaguePlayoffs/${playoffsId}`);

            var single = true;
            var bracketName = playoffs.FinalFullBracket.SubPlayoffBrackets[0].playoffName;
            if (playoffs.FinalFullBracket.SubPlayoffBrackets.length > 1) {
                single = false;
                var res = isInBracket(player, playoffsId);
                if (res) {
                    bracketName = res;
                }
                else {
                    return [];
                }
            }

            var trail = await axios.get(`/LeaguePlayoffs/${playoffsId}/${player}/${single}/${bracketName}/PlayoffRunTrail`);

            var playoffGames = [];
            for (var round in trail) {
                playoffGames.push([round.round, round.GameId]);
            }

            return playoffGames;
        };

        const processUserGames = async (username) => {
            const res = await axios.get(`/SingleGame/${username}`);

            var regGames = [];
            var seasonGames = [];
            var playoffGames = [];

            var league = await getPlayerLeague(username);

            var seasonGameList = [];

            var playoffGameList = [];

            if (league) {
                seasonGameList = await getSeasonGamesInLeague(username, league.SeasonAssignments);

                playoffGameList = await getPlayoffGamesInLeague(username, league.PlayoffAssignments);
            }

            for (var game in res.data) {
                if (league) {
                    if (seasonGameList.find(gameEntry => gameEntry.gameId == game.gameId) != null) {
                        seasonGames.push(game);
                    }
                    if (playoffGames.length > 0) {
                        for (var rd in playoffGames) {
                            if (rd[1].find(gameEntry => gameEntry == game.gameId) != null) {
                                playoffGames.push([rd[0], game]);
                            }
                        }
                    }
                } else {
                    regGames.push(game);
                }
            }

            regGames.sort((a, b) => {return a.timePlayed - b.timePlayed});
            seasonGames.sort((a, b) => {return a.timePlayed - b.timePlayed});

            playoffGames.sort((a, b) => {return a[1].timePlayed - b[1].timePlayed});

            return {
                regularGames : regGames,
                seasonGames : seasonGames,
                playoffGames : playoffGames
            };
        }

        const fetchData = async () => {
            var res = processUserGames(username);

            setCurrentGame(res.regGames[regGames.length-1]);

            if (league) {
                setCurrentSeasonUpcomingGame(seasonGames[seasonGames.length-1]);
                if (playoffGames.length > 0) {
                    setPlayoffUpcomingGame(playoffGames[playoffGames.length-1]);
                }
            }

            

        };

        fetchData();
    }, []);

};

export default Home;