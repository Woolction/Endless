import { useParams } from "react-router-dom";
import { useEffect, useState } from "react";
import VideoPlayer from "../components/video/VideoPlayer";
import { Comments } from "../components/Comments";
import { getContentById } from "../api/content.api";

export default function Watch() {
    const { id } = useParams(); const [video, setVideo] = useState(null);

    useEffect(() => { getContentById(id).then(setVideo); }, [id]);

    if (!video) return <div>Loading...</div>;

    return (<div className="flex gap-6"> <div className="flex-1"> <VideoPlayer src={video.contentResponseDto.contentUrl} />

        <h1 className="text-xl font-bold mt-4">
            {video.contentResponseDto.title}
        </h1>

        <Comments contentId={id} />
    </div>

        <div className="w-[350px]">Recommended</div>
    </div>

    );
}