import { Link, useLocation } from "react-router-dom";

export function Sidebar() {
    const { pathname } = useLocation();

    const Item = ({ to, icon, label }) => (
        <Link
            to={to}
            className={`flex items-center gap-4 px-4 py-2 rounded-lg hover:bg-gray-100 ${pathname === to ? "bg-gray-200" : ""
                }`}
        >
            <span>{icon}</span>
            <span className="text-sm">{label}</span>
        </Link>
    );

    return (
        <div className="w-60 h-screen overflow-y-auto border-r p-2">
            <Item to="/" icon="🏠" label="Home" />
            <Item to="/subscriptions" icon="📺" label="Subscriptions" />

            <hr className="my-2" />

            <Item to="/library" icon="📚" label="Library" />
            <Item to="/history" icon="🕒" label="History" />

            <hr className="my-2" />

            <div className="px-4 text-xs text-gray-500">Subscriptions</div>

            {[1, 2, 3].map((i) => (
                <div key={i} className="flex items-center gap-3 px-4 py-2 hover:bg-gray-100 rounded-lg">
                    <div className="w-6 h-6 bg-gray-300 rounded-full" />
                    <span className="text-sm">Channel {i}</span>
                </div>
            ))}
        </div>
    );
}
