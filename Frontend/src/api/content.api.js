import { api } from "./axios";

export const getRandomContent = async () => {
    const res = await api.get("/Content");
    return res.data.recoDto;
};

export const getRecommendedContent = async () => {
    const res = await api.get("/Content/recommendations");
    return res.data.recoDto;
};

export const getContentById = async (id) => {
    const res = await api.get(`/Content/${id}`);
    return res.data;
};