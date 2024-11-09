import { Routes, Route } from "react-router-dom";
import Register from "@/components/Register/Register.jsx";
import Login from "@/components/Login/Login.jsx";
import Missing from "@/components/Missing.jsx";
import Layout from "@/components/Layout.jsx";
import RequireAuth from "@/components/RequireAuth.jsx";
import PersistLogin from "@/components/PersistLogin.jsx";
import BasicExample from "@/components/NavBar.jsx";
import TaskPage from "@/components/TaskPage/TaskPage.jsx";
import WelcomePage from "@/components/WelcomePage/WelcomePage.jsx";
import CreateTask from "@/components/CreateTask/CreateTask.jsx";
import TaskResult from "@/components/TaskResult/TaskResult.jsx";

function App() {
    return (
        <div>
            <BasicExample />
            <Routes>
                <Route path="/" element={<Layout />}>
                    <Route index element={<WelcomePage />} />


                    <Route path="login" element={<Login />} />
                    <Route path="register" element={<Register />} />
                    <Route path="welcomepage" element={<WelcomePage />} />


                    <Route element={<PersistLogin />}>
                        <Route element={<RequireAuth />}>
                            <Route path="taskpage" element={<TaskPage />} />
                            <Route path="createtask" element={<CreateTask />} />
                            <Route path="taskresult/:id" element={<TaskResult />} />
                        </Route>
                    </Route>


                    <Route path="*" element={<Missing />} />
                </Route>
            </Routes>
        </div>
    );
}

export default App;
