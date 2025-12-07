/**
 * Chat Module - Handles messaging and conversation operations
 */

import { apiClient } from './api.js';

class ChatService {
  async sendMessage(receiverId, content, matchId = null) {
    return apiClient.post('/messages', {
      receiverId,
      content,
      matchId,
    });
  }

  async getConversation(otherUserId) {
    return apiClient.get(`/messages/conversation/${otherUserId}`);
  }

  async getConversations() {
    return apiClient.get('/messages/conversations');
  }

  async getConversationSummaries() {
    return apiClient.get('/messages/conversations/summaries');
  }

  async getInboxMessages() {
    return apiClient.get('/messages/inbox');
  }

  async getUnreadCount() {
    const result = await apiClient.get('/messages/unread-count');
    return result.count || 0;
  }

  startPolling(otherUserId, callback, interval = 2000) {
    let lastMessageId = null;
    
    const poll = async () => {
      try {
        const messages = await this.getConversation(otherUserId);
        if (messages && messages.length > 0) {
          const latestMessage = messages[messages.length - 1];
          if (latestMessage.id !== lastMessageId) {
            lastMessageId = latestMessage.id;
            callback(messages);
          }
        }
      } catch (error) {
        console.error('Polling error:', error);
      }
    };

    // Poll immediately
    poll();
    
    // Then poll at intervals
    const pollInterval = setInterval(poll, interval);
    
    return () => clearInterval(pollInterval);
  }
}

export const chatService = new ChatService();

