import VideoCard from "./VideoCard";

export default function VideoGrid({ videos }) {
    return (
        <div className="grid grid-cols-4 gap-4">
            {videos.Map((video) => (
                <VideoCard key={video.contentId} video={video} />
            ))}
        </div>
    );
}