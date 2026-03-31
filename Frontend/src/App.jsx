import { Routes, Route } from "react-router-dom";
import Home from "./pages/Home.jsx";
import VideoPage from "./pages/VideoPage.jsx";
import Navbar from "./components/layout/Navbar.jsx";

export default function App() {
    return (
        <div className="bg-[#0f0f0f] text-white min-h-screen flex">
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