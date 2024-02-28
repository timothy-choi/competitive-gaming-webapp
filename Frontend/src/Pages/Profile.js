import { useRef, useState, useEffect } from 'react';
import axios from './api/axios';
import { useParams } from 'react-router-dom';
import AuthContext from '../AuthProvider';

const Profile = () => {
    let { username } = useParams();

    const { auth } = useContext(AuthContext);

    const isMyProfile = auth.username === username;

    const [name, setName] = useState('');
    const [email, setEmail] = useState('');
    const [dateJoined, setDateJoined] = useState(null);
    const [isAvailable, setIsAvailable] = useState(false);
    const [playingGame, setPlayingGame] = useState(false);

    const [leagueJoined, setLeagueJoined] = useState(false);
    const [leagueName, setLeagueName] = useState('');

    const [playerFriends, setPlayerFriends] = useState([]);
    const [playerFriendCount, setPlayerFriendCount] = useState(null);

    const [playerRecord, setPlayerRecord] = useState([]);

    const [errorMessage, setErrorMessage] = useState('');

    const initUser = async () => {
        try {
            const response = await axios.get(`/Players/${username}`);

            setName(response.data.playerName);
            setEmail(response.data.playerEmail);
            setDateJoined(response.data.playerJoined);
            setIsAvailable(response.data.playerAvailable);
            setPlayingGame(response.data.playerInGame);
            setPlayerFriendCount(response.data.playerFriends.length);
            setLeagueJoined(response.data.leagueJoined);
            setLeagueName(response.data.playerLeagueJoined);
            setPlayerRecord(response.data.singlePlayerRecord);
            var allFriends = [];
            for (let i = 0; i < response.data.playerFriends.length; ++i) {
                var friend = await axios.get(`/Players/${response.data.playerFriends[i]}`);
                var friendInfo = {
                    name: friend.data.name,
                    username: friend.data.username,
                    isAvailable: friend.data.playerAvailable,
                    playingGame: friend.data.playerInGame
                };
                allFriends.push(friendInfo);
            }
            setPlayerFriends(allFriends);

            
        } catch (e) {
            setErrorMessage(`${username} doesn't exist`);
        }
    }

    useEffect(() => {
        initUser();
    }, []);

}

export default Profile;