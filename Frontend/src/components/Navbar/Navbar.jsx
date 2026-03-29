export default function Navbar() {
    return (
        <div className="sticky top-0 z-50 bg-[#0f0f0f]/80 backdrop-blur flex items-center justify-between px-6 py-3 border-b border-white/10">
            <h1 className="text-2xl font-bold tracking-tight">Endless</h1>
            <input
                placeholder="Search..."
                className="bg-[#1a1a1a] px-4 py-2 rounded-full w-96 outline-none focus:ring-2 focus:ring-white/20"
            />
            <div className="w-8 h-8 bg-gradient-to-br from-purple-500 to-pink-500 rounded-full" />
        </div>
    );
}
