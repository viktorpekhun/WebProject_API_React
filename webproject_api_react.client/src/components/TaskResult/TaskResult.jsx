import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import axios from "@/api/axios.jsx";
import './TaskResult.css';

const TaskResult = () => {
    const { id } = useParams();  // Отримуємо ID з URL
    const navigate = useNavigate(); // Для переходу на попередню сторінку
    const [taskData, setTaskData] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        const fetchTaskResult = async () => {
            try {
                const response = await axios.get(`/api/task/result/${id}`);
                setTaskData(response.data);
            } catch (err) {
                if (!err?.response) {
                    setError("No server response.");
                } else if (err.response.status === 404) {
                    setError("Task not found.");
                } else {
                    setError("Failed to fetch task result.");
                }
            } finally {
                setLoading(false);
            }
        };

        fetchTaskResult();
    }, [id]);

    const handleBackClick = () => {
        navigate(-1);
    };

    if (loading) return <p>Loading...</p>;
    if (error) return <p className="errmsg">{error}</p>;


    const parameters = taskData.parameters ? JSON.parse(taskData.parameters) : {};

    return (
        <section className="task-result-container">
            <div className="task-result">

                <h1>Task Result</h1>
                <div className="task-info">
                    <p><strong>Task ID:</strong> {taskData.taskId}</p>
                    <p><strong>Status:</strong> {taskData.taskStatus}</p>
                    <p><strong>Created At:</strong> {new Date(taskData.createdAt).toLocaleString()}</p>
                    <p><strong>Updated At:</strong> {new Date(taskData.updatedAt).toLocaleString()}</p>
                    <p><strong>Result:</strong> {taskData.result ?? "No result yet"}</p>
                    {taskData.errorMessage && <p><strong>Error:</strong> {taskData.errorMessage}</p>}

                    <div className="parameters">
                        <strong>Parameters:</strong>
                        <ul>
                            {Object.keys(parameters).map(key => (
                                <li key={key}><strong>{key}:</strong> {parameters[key]}</li>
                            ))}
                        </ul>
                    </div>
                </div>
                <button className="back-btn" onClick={handleBackClick}>Go Back</button>
            </div>
        </section>
    );
};

export default TaskResult;
