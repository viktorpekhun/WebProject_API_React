import { useState, useEffect, useRef } from "react";
import { faCheck, faTimes, faInfoCircle } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import axios from "@/api/axios.jsx";
import { useNavigate } from "react-router-dom";
import useAuth from "@/hooks/useAuth.jsx";
import './CreateTask.css'

const TASK_PARAM_REGEX = /^[1-9]\d{0,3}$/;
const CreateTask = () => {
    const { auth } = useAuth();
    const id = Number(auth?.userId);
    const navigate = useNavigate();

    const paramRef = useRef();
    const errRef = useRef();

    const [taskType, setTaskType] = useState("FactorialCalculation");
    const [parameter, setParameter] = useState("");
    const [validParameter, setValidParameter] = useState(false);
    const [paramFocus, setParamFocus] = useState(false);

    const [errMsg, setErrMsg] = useState("");

    useEffect(() => {
        paramRef.current.focus();
    }, []);

    useEffect(() => {
        const result = TASK_PARAM_REGEX.test(parameter);
        setValidParameter(result);
    }, [parameter]);

    useEffect(() => {
        setErrMsg("");
    }, [taskType, parameter]);

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!validParameter) {
            setErrMsg("Invalid parameter value. Must be between 1 and 4000.");
            return;
        }

        try {
            const response = await axios.post(
                "/api/task/create",
                JSON.stringify({
                    UserId: id,
                    TaskType: taskType,
                    Parameters: { number: parameter },
                }),
                {
                    headers: { "Content-Type": "application/json" },
                    withCredentials: true,
                }
            );

            setParameter("");
            navigate("/taskpage");
        } catch (err) {
            if (!err?.response) {
                setErrMsg("No Server Response");
            } else {
                setErrMsg("Failed to create task");
            }
            errRef.current.focus();
        }
    };

    return (
        <section className="create-task-container">
            <p ref={errRef} className={errMsg ? "errmsg" : "offscreen"} aria-live="assertive">
                {errMsg}
            </p>
            <h1>Create Task</h1>
            <form onSubmit={handleSubmit}>
                <label htmlFor="taskType">Task Type:</label>
                <select
                    id="taskType"
                    value={taskType}
                    onChange={(e) => setTaskType(e.target.value)}
                    required
                >
                    <option value="FactorialCalculation">Factorial Calculation</option>
                    <option value="Calculation">Calculation</option>
                </select>

                <label htmlFor="parameter">
                    Parameter (1-4000):
                    <span className={validParameter ? "valid" : "hide"}>
                        <FontAwesomeIcon icon={faCheck} />
                    </span>
                    <span className={validParameter || !parameter ? "hide" : "invalid"}>
                        <FontAwesomeIcon icon={faTimes} />
                    </span>
                </label>
                <input
                    type="number"
                    id="parameter"
                    ref={paramRef}
                    value={parameter}
                    onChange={(e) => setParameter(e.target.value)}
                    required
                    aria-invalid={validParameter ? "false" : "true"}
                    aria-describedby="paramnote"
                    onFocus={() => setParamFocus(true)}
                    onBlur={() => setParamFocus(false)}
                />
                <p id="paramnote" className={paramFocus && parameter && !validParameter ? "instructions" : "offscreen"}>
                    <FontAwesomeIcon icon={faInfoCircle} />
                    Must be a number between 1 and 4000.
                </p>

                <button disabled={!validParameter}>Create Task</button>
            </form>
        </section>
    );
};

export default CreateTask;
