import { useEffect, useState } from "react";
import { api } from "../api";

export default function Comments({ contentId }) {
    const [comments, setComments] = useState([]);
    const [text, setText] = useState("");

    const load = () => {
        api.get(`/Comment/content/${contentId}`).then((res) => setComments(res.data));
    };

    useEffect(() => {
        load();
    }, [contentId]);

    const send = async () => {
        await api.post(`/Comment/content/${contentId}`, { text });
        setText("");
        load();
    };

    return (
        <div className="mt-6">
            <h2 className="font-semibold mb-4">Comments</h2>

            <div className="flex gap-2 mb-4">
                <input
                    value={text}
                    onChange={(e) => setText(e.target.value)}
                    className="flex-1 bg-[#1a1a1a] p-3 rounded-full outline-none"
                    placeholder="Add a comment..."
                />
                <button
                    onClick={send}
                    className="px-4 bg-white text-black rounded-full"
                >
                    Send
                </button>
            </div>

            <div className="space-y-4">
                {comments.map((c) => (
                    <div key={c.commentResponseDto.commentId} className="flex gap-3">
                        <div className="w-8 h-8 rounded-full bg-gray-600" />
                        <div>
                            <p className="text-sm font-semibold">{c.user.name}</p>
                            <p className="text-gray-300 text-sm">
                                {c.commentResponseDto.text}
                            </p>
                        </div>
                    </div>
                ))}
            </div>
        </div>
    );
}
