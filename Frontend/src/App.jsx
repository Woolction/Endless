import { Routes, Route } from "react-router-dom";
import Home from "./Pages/Home.jsx";
import VideoPage from "./Pages/VideoPage.jsx";
import Navbar from "./components/Navbar/Navbar.jsx";
import Sidebar from "./components/Side.jsx";

export default function App() {
    return (
        <div className="bg-[#0f0f0f] text-white min-h-screen flex">
            <Sidebar />
            <div className="flex-1">
                <Navbar />
                <Routes>
                    <Route path="/" element={<Home />} />
                    <Route path="/video/:id" element={<VideoPage />} />
                </Routes>
            </div>
        </div>
    );
}