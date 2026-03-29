import { useNavigate } from "react-router-dom";

export default function VideoCard({ video }) {
    const nav = useNavigate();

    return (
        <div
            onClick={() => nav(`/video/${video.contentId}`)}
            className="group cursor-pointer"
        >
            <div className="relative overflow-hidden rounded-2xl">
                <img
                    src={video.prewievPhotoUrl}
                    className="w-full h-48 object-cover group-hover:scale-110 transition duration-500"
                />
                <div className="absolute bottom-2 right-2 bg-black/80 text-xs px-2 py-1 rounded">
                    {video.durationSeconds}s
                </div>
            </div>

            <div className="mt-3 flex gap-3">
                <div className="w-10 h-10 rounded-full bg-gradient-to-br from-purple-500 to-pink-500" />
                <div>
                    <h3 className="font-semibold leading-tight group-hover:text-gray-300">
                        {video.title}
                    </h3>
                    <p className="text-sm text-gray-400">{video.viewsCount} views</p>
                </div>
            </div>
        </div>
    );
}
