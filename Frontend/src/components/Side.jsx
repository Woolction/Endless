export default function Sidebar() {
    return (
        <div className="w-64 p-4 border-r border-white/10 hidden md:block">
            <nav className="space-y-2">
                <div className="p-3 rounded-xl hover:bg-white/10 cursor-pointer">Home</div>
                <div className="p-3 rounded-xl hover:bg-white/10 cursor-pointer">Trending</div>
                <div className="p-3 rounded-xl hover:bg-white/10 cursor-pointer">Subscriptions</div>
            </nav>
        </div>
    );
}