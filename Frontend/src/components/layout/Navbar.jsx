import { Link } from "react-router-dom";
import { useState } from "react";

export function Navbar() {
    const [query, setQuery] = useState("");

    const toggleTheme = () => {
        const isDark = document.documentElement.classList.toggle("dark");
        localStorage.setItem("theme", isDark ? "dark" : "light");
    };

    return (<div className="flex items-center justify-between px-4 h-14 border-b bg-white dark:bg-[#0f0f0f] sticky top-0 z-50"> {/* Left */} <div className="flex items-center gap-4"> <button className="text-xl">☰</button> <Link to="/" className="font-bold text-lg">Endless</Link> </div>

        {/* Center */}
        <div className="flex items-center w-[40%]">
            <input
                value={query}
                onChange={(e) => setQuery(e.target.value)}
                placeholder="Search"
                className="w-full border dark:border-gray-700px-3 py-1 rounded-l-full outline-none"
            />
            <button className="px-4 border dark:border-gray-700rounded-r-full bg-gray-100">
                🔍
            </button>
        </div>

        {/* Right */}
        <div className="flex items-center gap-4">
            <button onClick={toggleTheme}>🌙</button>
            <button>📹</button>
            <button>🔔</button>
            <div className="w-8 h-8 bg-gray-300 rounded-full" />
        </div>
    </div>

    );
}
