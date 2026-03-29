import { useState } from "react";
import { api } from "../api.js";

export default function Login() {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");

    const login = async () => {
        const res = await api.post("/Auth/token", { email, password });
        localStorage.setItem("token", res.data.token);
        window.location.href = "/";
    };

    return (
        <div className="flex justify-center items-center h-screen">
            <div className="bg-gray-900 p-6 rounded-xl w-80">
                <h2 className="text-xl mb-4">Login</h2>
                <input placeholder="email" onChange={(e) => setEmail(e.target.value)} className="w-full mb-2 p-2 bg-gray-800" />
                <input placeholder="password" type="password" onChange={(e) => setPassword(e.target.value)} className="w-full mb-4 p-2 bg-gray-800" />
                <button onClick={login} className="w-full bg-white text-black p-2 rounded">Login</button>
            </div>
        </div>
    );
}