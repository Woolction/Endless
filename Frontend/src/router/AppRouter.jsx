import { BrowserRouter, Routes, Route } from "react-router-dom";
import Home from "../pages/Home";
import Watch from "../pages/Watch";
import { Layout } from "../components/layout/Layout";

export default function AppRouter() {
    const savedTheme = localStorage.getItem("theme");

    if (savedTheme === "dark") {
        document.documentElement.classList.add("dark");
    }
    
    return (
        <BrowserRouter>
            <Layout>
                <Routes>
                    <Route path="/" element={<Home />} />
                    <Route path="/watch/:id" element={<Watch />} />
                </Routes>
            </Layout>
        </BrowserRouter>);
}
