import { useEffect, useState } from "react";
import { api } from "../api/axios";

export function Comments({ contentId }) {
    const [comments, setComments] = useState([]); const [text, setText] = useState("");

    useEffect(() => {
        api.get(`/Comment/content/{ contentId }`).then((res) => {
            setComments(res.data);
        });
    }, [contentId]);

    const send = async () => {
        const res = await api.post(`/Comment/content/${contentId}`, { text });
        setComments([res.data, ...comments]); setText("");
    };

    return (<div className="mt-6"> <h2 className="font-bold mb-2">Comments</h2>

        <div className="flex gap-2 mb-4">
            <input
                value={text}
                onChange={(e) => setText(e.target.value)}
                className="border dark:border-gray-700flex-1 px-2 py-1"
                placeholder="Write a comment..."
            />
            <button onClick={send} className="bg-blue-500 text-white px-4">
                Send
            </button>
        </div>

        {comments.map((c) => (
            <div key={c.commentId} className="mb-2">
                <p className="text-sm">{c.text}</p>
                <span className="text-xs text-gray-400">{c.likeCount} likes</span>
            </div>
        ))}
    </div>

    );
}