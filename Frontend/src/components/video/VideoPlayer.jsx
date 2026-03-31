import { useEffect, useRef } from "react";
import Hls from "hls.js";

export default function VideoPlayer({ src }) {
    const videoRef = useRef < HTMLVideoElement > (null);

    useEffect(() => {
        if (!videoRef.current) return;

        if (Hls.isSupported()) {
            const hls = new Hls();
            hls.loadSource(src);
            hls.attachMedia(videoRef.current);
        } else {
            videoRef.current.src = src;
        }
    }, [src]);

    return (
        <video
            ref={videoRef}
            controls
            className="w-full rounded-xl"
        />
    );
}