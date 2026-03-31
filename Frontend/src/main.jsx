//import React from "react";
//import ReactDOM from "react-dom/client";
//import { BrowserRouter } from "react-router-dom";
//import App from "./App";
//import Home from "./pages/Home";
//import "./index.css";

//ReactDOM.createRoot(document.getElementById("root")).render(
//  <React.StrictMode>
//    <BrowserRouter>
//      <Home />
//    </BrowserRouter>
//  </React.StrictMode>
//);

import React from "react";
import ReactDOM from "react-dom/client";
import AppRouter from "./router/AppRouter";
import "./index.css";

ReactDOM.createRoot(document.getElementById("root")).render(
  <React.StrictMode>
    <AppRouter />
  </React.StrictMode>);