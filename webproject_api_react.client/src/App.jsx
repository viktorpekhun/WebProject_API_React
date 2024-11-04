import {Routes, Route} from "react-router-dom";
import Register from "@/components/Register.jsx";
import Login from "@/components/Login.jsx";
import Home from "@/components/Home.jsx";
import Missing from "@/components/Missing.jsx";
import LinkPage from "@/components/LinkPage.jsx";
import Layout from "@/components/Layout.jsx";
import RequireAuth from "@/components/RequireAuth.jsx";
import Lounge from "@/components/Lounge.jsx";
import PersistLogin from "@/components/PersistLogin.jsx";



function App() {
    return(
        <Routes>
            <Route path="/" element={<Layout />}>
                {/* Redirect root to LinkPage */}
                <Route index element={<LinkPage />} />

                {/* Public routes */}
                <Route path="login" element={<Login />} />
                <Route path="register" element={<Register />} />
                <Route path="linkpage" element={<LinkPage />} />

                {/* Protected routes */}
                <Route element={<PersistLogin />}>
                    <Route element={<RequireAuth />}>
                        <Route path="home" element={<Home />} />
                        <Route path="lounge" element={<Lounge />} />
                    </Route>
                </Route>

                {/* Catch-all route for unmatched paths */}
                <Route path="*" element={<Missing />} />
            </Route>
        </Routes>
    );
}

export default App;