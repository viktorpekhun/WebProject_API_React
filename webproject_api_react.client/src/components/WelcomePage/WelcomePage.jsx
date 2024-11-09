import React from 'react';
import './WelcomePage.css';
import {Link} from "react-router-dom";

const WelcomePage = () => {
    return (
        <div className="welcome-container">
            <h1 className="welcome-header">Welcome!</h1>
            <p className="welcome-text">To continue, please click the link below.</p>
            <span className="line">
                <Link to="/taskpage">Continue work</Link>
            </span>
        </div>
    );
};

export default WelcomePage;