import { useEffect, useState } from "react";
import VideoGrid from "../components/video/VideoGrid";
import {
    getRandomContent,
    getRecommendedContent,
} from "../api/content.api";

export default function Home() {
    const [videos, setVideos] = useState([]);

    useEffect(() => {
        const load = async () => {
            const token = localStorage.getItem("token");

            const data = token
                ? await getRandomContent() : await getRecommendedContent();

            setVideos(data);
        };

        load();
    }, []);

    return <VideoGrid videos={videos} />;
}