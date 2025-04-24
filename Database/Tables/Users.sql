CREATE TABLE Users (
    user_id BIGINT PRIMARY KEY,
    username NVARCHAR(255),
    is_bot BIT,
    avatar_url NVARCHAR(1000) NULL
);
