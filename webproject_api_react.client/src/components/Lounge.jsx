import { Link } from "react-router-dom"
import Users from "@/components/Users.jsx";

const Lounge = () => {
    return (
        <section>
            <h1>The Lounge</h1>
            <br />
            <Users />
            <br />
            <div className="flexGrow">
                <Link to="/home">Home</Link>
            </div>
        </section>
    )
}

export default Lounge