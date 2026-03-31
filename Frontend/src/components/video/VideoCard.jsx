import { Link } from "react-router-dom";

export default function VideoCard({ video }) {
    return (
        <Link to={`/watch/${video.contentId}`} className="block">
            <img
                src={video.prewievPhotoUrl}
                className="rounded-xl w-full"
            />

            <div className="mt-2">
                <h3 className="text-sm font-semibold line-clamp-2">
                    {video.title}
                </h3>

                <p className="text-xs text-gray-600 dark:text-gray-400">
                    {video.viewsCount} views
                </p>
            </div>
        </Link>
    );
}