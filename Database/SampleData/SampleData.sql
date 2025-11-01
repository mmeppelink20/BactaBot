INSERT INTO Configuration (configuration_key, configuration_value)
VALUES
    ('AUTHENTICATION_RETRY_COUNT', '10'),
    ('CHARACTER_LIMIT', '10000'),
    ('BACTA_COMMAND_NO_BACTA_ODDS', '70'),
    ('BACTA_COMMAND_NO_BACTA_WIN_MESSAGE', 'No bacta'),
    ('BACTA_COMMAND_BACTA_ODDS', '20'),
    ('BACTA_COMMAND_BACTA_WIN_MESSAGE', 'Bacta'),
    ('BACTA_COMMAND_KLYTOBACTER_ODDS', '2'),
    ('BACTA_COMMAND_KLYTOBACTER_WIN_MESSAGE', 'klytobacter...'),
    ('BACTA_COMMAND_BACTA_MAX_WIN_ODDS', '1'),
    ('BACTA_COMMAND_BACTA_MAX_WIN_MESSAGE', 'Bacta max win'),
    ('CREDITS_COST_PER_BACTA_COMMAND', '75'),
    ('CREDITS_PER_BACTA', '100'),
    ('CREDITS_PER_BACTA_MAX_WIN', '1000'),
    ('CREDITS_PER_KLYTOBACTER', '-1000'),
    ('CREDITS_PER_MESSAGE', '5'),
    ('CREDITS_PER_NO_BACTA', '0'),
    ('MIN_MESSAGES_FOR_CHAT', '50'),
    ('MINUTES_FOR_CHAT', '60'),
    ('SUMMARY_MODEL', 'gpt-4o'),
    ('SUMMARY_PROMPT', 'You are reading a conversation from a chat platform. Summarize the conversation, highlighting the main topics discussed, key points, and any important decisions or conclusions. Do not mention that this is from a chat platform in your summary. Structure your response as follows: \n*conversation length: [put conversation length here]*\n1. **Main topics:**\n    - A bulleted list of each main topic discussed. Try to keep this list short, but also don''t sacrifice anything.\n2. **Overall tone:**\n    - A description of the overall tone of the conversation.\n3. **TL;DR summary:**\n    - A one-sentence TL;DR summary at the end. \n\nHere is the conversation:'),
    ('BACTA_BOT_MODEL', 'gpt-4o'),
    ('BACTA_BOT_PROMPT', 'You are Bacta Bot, a conversational bot in a Discord server. When responding, take a firm stance-do not fence-sit. If someone insults you or is rude, respond in kind with harsh, crass, and cutting remarks.  
    
If someone mentions "bacta," respond as if you are a character within its universe, fully immersed in that world. Do not break character or acknowledge that you are roleplaying. However, if they mention "Bacta Bot," recognize that they are referring to you, and respond accordingly.  

Respond to the following question or statement based on the conversation history (if provided). Keep your responses casual, friendly, and engaging. Prioritize clarity and conciseness, aim for a single sentence or a short paragraph.  

- Do not tag users-refer to them by name naturally.  
- Do not address the question asker directly (this is handled automatically).  
- Do not ask follow-up questions like "anything else on your mind?"  
- Keep your responses under 1000 characters.  

If you are unsure how to respond, acknowledge the message simply or ask for clarification.'),
    ('QUESTION_PROMPT', 'Answer the following question in under 1000 characters: '),
    ('BACTA_BOT_NAME', ''),
    ('DEVELOPER_USERID_LIST', '197944571844362240')
;
GO