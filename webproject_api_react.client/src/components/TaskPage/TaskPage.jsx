import React, { useState, useEffect, useCallback } from 'react';
import axios from "@/api/axios.jsx";
import './TaskPage.css';
import useAuth from "@/hooks/useAuth.jsx";
import { Link } from "react-router-dom";

const HISTORY_URL = '/api/task/history';
const RESTART_URL = '/api/task/restart';
const CANCEL_URL = '/api/task/cancel';

const TaskPage = () => {
    const { auth } = useAuth();
    const userId = Number(auth?.userId);
    const [tasks, setTasks] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);

    // Функція для отримання завдань з API
    const fetchTasks = useCallback(async () => {
        try {
            const response = await axios.get(HISTORY_URL, {
                params: { userId, batchSize: 100 }
            });

            const newTasks = response.data;


            setTasks((prevTasks) => {
                const taskMap = new Map(prevTasks.map(task => [task.id, task]));
                newTasks.forEach(task => {
                    taskMap.set(task.id, task);
                });
                return Array.from(taskMap.values()).sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));
            });
        } catch (err) {
            setError('Failed to fetch tasks');
        }
    }, [userId]);


    useEffect(() => {
        fetchTasks(); // Перший запит
        const intervalId = setInterval(fetchTasks, 1500);
        return () => clearInterval(intervalId);
    }, [fetchTasks]);

    const handleCancelTask = async (taskId) => {
        try {
            await axios.post(`${CANCEL_URL}/${taskId}`);
            setTasks(prevTasks =>
                prevTasks.map(task =>
                    task.id === taskId ? { ...task, status: 'Cancelled' } : task
                )
            );
        } catch (error) {
            alert('Failed to cancel task.');
        }
    };

    const handleRestartTask = async (taskId) => {
        try {
            await axios.post(`${RESTART_URL}/${taskId}`);
            setTasks(prevTasks =>
                prevTasks.map(task =>
                    task.id === taskId ? { ...task, status: 'Pending' } : task
                )
            );
        } catch (error) {
            alert('Failed to restart task.');
        }
    };

    if (error) {
        return <div>{error}</div>;
    }

    return (
        <div className="task-page-container">
            <div className="task-page">
                <div className="header-container">
                    <h1>Task History</h1>
                    <button className="create-task-btn">
                        <Link to="/createtask">Create New Task</Link>
                    </button>
                </div>

                <div className="table-container">
                    <table>
                        <thead>
                        <tr>
                            <th>ID</th>
                            <th>Type</th>
                            <th>Creation Date</th>
                            <th>Result</th>
                            <th>Status</th>
                            <th>Actions</th>
                        </tr>
                        </thead>
                        <tbody>
                        {tasks.map((task) => (
                            <tr key={task.id}>
                                <td>{task.id}</td>
                                <td>{task.taskType}</td>
                                <td>{new Date(task.createdAt).toLocaleString()}</td>
                                <td>{task.result || "N/A"}</td>
                                <td>{task.status}</td>
                                <td className="actions-cell">
                                    <button className="details-btn">
                                        <Link to={`/taskresult/${task.id}`}>Details</Link>
                                    </button>
                                    {task.status === "Pending" || task.status === "InProgress" || task.status === "InQueue" ? (
                                        <button className="cancel-btn" onClick={() => handleCancelTask(task.id)}>Cancel</button>
                                    ) : (task.status === "Completed" || task.status === "Cancelled") ? (
                                        <button className="restart-btn" onClick={() => handleRestartTask(task.id)}>Restart</button>
                                    ) : null}
                                </td>
                            </tr>
                        ))}
                        </tbody>
                    </table>
                </div>
                {loading && <div>Loading...</div>}
            </div>
        </div>
    );
};

export default TaskPage;
