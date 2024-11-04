import {useState, useEffect} from "react";
import useAxiosPrivate from "@/hooks/useAxiosPrivate.jsx";
import {useLocation, useNavigate} from "react-router-dom";

const Users = () => {
    const [users, setUsers] = useState();
    const axiosPrivate = useAxiosPrivate();
    const navigate = useNavigate();
    const location = useLocation();

    useEffect(() => {
        let isMounted = true;
        const controller = new AbortController();

        const getUsers = async () => {

            try {
                const response = await axiosPrivate.get('/api/auth/users', {
                    signal: controller.signal
                });
                console.log(response.data);
                isMounted && setUsers(response.data);
            } catch (err) {
                if (err.name === 'CanceledError') {
                    // Ігноруємо помилку скасування
                    return;
                }
                console.error(err); // Обробляємо інші помилки
                navigate('/login', { state: { from: location }, replace: true});
            }
        }
        getUsers();

        return () => {
            isMounted = false;
            controller.abort();
        }
    }, [])

    return (
        <article>
            <h2>Users List</h2>
            {users?.length
                ? (
                    <ul>
                        {users.map((user, i) => <li key={i}>{user?.username}</li>)}
                    </ul>
                ) : <p>No users to display</p>
            }
        </article>
    );
};

export default Users;

