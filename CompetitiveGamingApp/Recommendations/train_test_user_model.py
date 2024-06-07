import numpy as np


class UserIncrementalSVD:
    def __init__(self, n_factors=10, learning_rate=0.005, regularization=0.02, n_epochs=30):
        self.n_factors = n_factors
        self.learning_rate = learning_rate
        self.regularization = regularization
        self.n_epochs = n_epochs
        self.U = None
        self.V = None
        self.user_ids = {}
        self.league_ids = {}
        self.user_count = 0
        self.league_count = 0
    
    def fit(self, user_matrix):
        n_users, n_items = user_matrix.shape

        combined_matrix = user_matrix.reshape(n_users, -1)

        scale_factor = 1.0/self.n_factors

        self.U = np.random.normal(scale=scale_factor, size=(n_users, self.n_factors))
        self.V = np.random.normal(scale=scale_factor, size=(n_items, self.n_factors))

        for epoch in range(self.n_epochs):
            self.sgd(combined_matrix)
    
    def sgd(self, combined_matrix):
        n_users, n_items_flattened = combined_matrix.shape

        for user in range(n_users):
            for item in range(n_items_flattened):
                if combined_matrix[user, item] > 0:
                    pred = self.predict(user, item % self.V.shape[0])
                    err = combined_matrix[user, item] - pred

                    self.U[user, :] += self.learning_rate * (err * self.V[item % self.V.shape[0], :] - self.regularization * self.U[user, :])
                    self.V[item % self.V.shape[0], :] += self.learning_rate * (err * self.U[user, :] - self.regularization * self.V[item % self.V.shape[0], :])

    def predict(self, user_index, item_index):
        return self.U[user_index, :].dot(self.V[item_index, :])

    def recommend(self, user_index, user_item_matrix, num_recommendations=5):
        user_similarity = self.U.dot(self.U[user_index,:])
        recommendations = []

        for user_idx in range(len(self.U)):
            if user_idx != user_idx:
                similarity = user_similarity[user_idx]
                for league_idx in range(len(self.V)):
                    prediction = self.predict(user_idx, league_idx)
                    weighted_pred = prediction  * similarity
                    recommendations.append((user_idx, league_idx, weighted_pred))
        
        recommendations.sort(key=lambda x: x[2], reverse=True)
        
        filtered_recommendations = [
            (user_idx, league_idx, prediction) 
            for user_idx, league_idx, prediction in recommendations
            if user_item_matrix[user_index, league_idx] == 0
        ]

        return filtered_recommendations[:num_recommendations]

    def add_data(self, username, league, wins, losses, league_tags, user_matrix):
        if username not in self.user_ids:
            self.user_ids[username] = self.user_count
            self.user_count += 1
            new_user_factor = np.random.normal(scale=1./self.n_factors, size=(1, self.n_factors))
            self.U = np.vstack([self.U, new_user_factor])
        if league not in self.league_ids:
            self.league_ids[league] = self.league_count
            self.league_count += 1
            new_league_factor = np.random.normal(scale=1./self.n_factors, size=(1, self.n_factors))
            self.V = np.vstack([self.V, new_league_factor])
        
        user_idx = self.user_ids[username]
        league_idx = self.league_ids[league]

        user_matrix[user_idx, league_idx, 0] = wins
        user_matrix[user_idx, league_idx, 1] = losses
        user_matrix[user_idx, league_idx, 2:] = tags_encoded[_]

        tags_encoded = np.zeros(len(league_tags), dtype=int)

        data_point = np.array([wins, losses] + tags_encoded.tolist())

        self.sgd_incremental(user_idx, league_idx, data_point)

        return user_matrix

    def sgd_incremental(self, user_idx, league_idx, data_point):
        n_features = data_point.size

        for feature in range(n_features):
            if data_point[feature] > 0:
                prediction = self.predict(user_idx, league_idx)
                error = data_point[feature] - prediction

                self.U[user_idx, :] += self.learning_rate * (error * self.V[league_idx, :] - self.regularization * self.U[user_idx, :])
                self.V[league_idx, :] += self.learning_rate * (error * self.U[user_idx, :] - self.regularization * self.V[league_idx, :])



def create_combined_matrix(dframe, tags_encoded):
    users = dframe['Username'].unique()
    leagues = dframe['League'].unique()

    user_ids = {user: idx for idx, user in enumerate(users)}
    league_ids = {league: idx for idx, league in enumerate(leagues)}

    num_tags = tags_encoded.shape[1]

    user_item_matrix = np.zeros((len(users), len(leagues), 2 + num_tags))

    for _, row in dframe.iterrows():
        user_idx = user_ids[row['Username']]
        league_idx = league_ids[row['League']]
        user_item_matrix[user_idx, league_idx, 0] = row['Wins']
        user_item_matrix[user_idx, league_idx, 1] = row['Losses']
        user_item_matrix[user_idx, league_idx, 2:] = tags_encoded[_]
    
    return user_item_matrix, user_ids, league_ids