import { useEffect, useState } from "react";
import { api } from "../api";
import VideoCard from "../components/VideoCard";

export default function Home() {
    const [videos, setVideos] = useState([]);

    useEffect(() => {
        api.get("/Content").then((res) => setVideos(res.data.recoDto));
    }, []);

    return (
        <div className="p-6 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
            {videos.map((v) => (
                <VideoCard key={v.contentId} video={v} />
            ))}
        </div>
    );
}
