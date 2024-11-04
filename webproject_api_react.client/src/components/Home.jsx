import { useNavigate, Link } from "react-router-dom";
import useLogout from "@/hooks/useLogout.jsx";

const Home = () => {
    const navigate = useNavigate();
    const logout = useLogout();

    const signOut = async () => {
        await logout();
        navigate('/linkpage');
    }

    /*
    const logout = async () => {
        try {
            // Виконання запиту до ендпоінту logout
            await axios.post('/api/auth/logout', {}, { withCredentials: true });
            // Очистка контексту аутентифікації
            setAuth({});
            // Перенаправлення на сторінку
            navigate('/linkpage');
        } catch (error) {
            console.error("Logout failed:", error);
            // Можна додати обробку помилок, наприклад, показати повідомлення
        }
    }
    */

    return (
        <section>
            <h1>Home</h1>
            <br />
            <p>You are logged in!</p>
            <br />
            <Link to="/editor">Go to the Editor page</Link>
            <br />
            <Link to="/admin">Go to the Admin page</Link>
            <br />
            <Link to="/lounge">Go to the Lounge</Link>
            <br />
            <Link to="/linkpage">Go to the link page</Link>
            <div className="flexGrow">
                <button onClick={signOut}>Sign Out</button>
            </div>
        </section>
    )
}

export default Home