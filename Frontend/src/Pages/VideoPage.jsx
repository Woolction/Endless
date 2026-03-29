import { useParams } from "react-router-dom";
import { useEffect, useState } from "react";
import { api } from "../api";
import Comments from "../components/Comments";

export default function VideoPage() {
    const { id } = useParams();
    const [video, setVideo] = useState(null);

    useEffect(() => {
        api.get(`/Content/${id}`).then((res) => {
            setVideo(res.data.contentResponseDto);
        });
    }, [id]);

    if (!video) return <div className="p-6">Loading...</div>;

    return (
        <div className="p-6 max-w-6xl mx-auto grid grid-cols-1 lg:grid-cols-3 gap-6">
            <div className="lg:col-span-2">
                <video controls className="w-full rounded-2xl shadow-lg">
                    <source src={video.contentUrl} />
                </video>

                <h1 className="text-2xl font-bold mt-4">{video.title}</h1>

                <div className="flex gap-3 mt-3">
                    <button className="px-4 py-2 bg-white text-black rounded-full hover:scale-105 transition">
                        Like
                    </button>
                    <button className="px-4 py-2 bg-white/10 rounded-full hover:bg-white/20">
                        Save
                    </button>
                </div>

                <Comments contentId={id} />
            </div>

            <div className="space-y-4">
                <p className="text-gray-400">Recommended (coming soon)</p>
            </div>
        </div>
    );
}
