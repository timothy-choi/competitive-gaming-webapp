import numpy as np
from sklearn.linear_model import SGDClassifier
from sklearn.preprocessing import LabelEncoder
from data_league_preprocessing import *

def fit(records):
    all_tags = find_common_tags(records)
    X_train = np.array([extract_features(league, all_tags) for league in records]) 
    y_train = np.array([league.LeagueName for league in records])

    label_encoder = LabelEncoder()
    y_train_encoded = label_encoder.fit_transform(y_train)

    model = SGDClassifier(loss='log', max_iter=1000, tol=1e-3)

    model.partial_fit(X_train, y_train_encoded, classes=np.unique(y_train_encoded))

    return model, label_encoder, all_tags

def recommend_leagues(model, available_leagues, all_tags, label_encoder, top_n=5):
    league_scores = []

    for league in available_leagues:
        features = extract_features(league, all_tags)

        scores = model.predict_proba([features])[0]

        score = 0

        if league.LeagueNames in label_encoder.classes_:
            league_index = label_encoder.transform([league.LeagueName])[0]
            score = scores[league_index]
        
        league_scores.append((league.LeagueName, score))
    
    league_scores.sort(key=lambda x: x[1], reverse=True)

    return [league for league, score in league_scores[:top_n]]



def add_data(model, all_tags, label_encoder, new_record):
    if new_record.LeagueName not in label_encoder.classes_:
        label_encoder.classes_ = np.append(label_encoder.classes_, new_record.LeagueName)

    X_new = np.array([extract_features(new_record, all_tags)])

    y_new = label_encoder.transform([new_record.LeagueName])

    model.partial_fit(X_new, y_new)

    return model, label_encoder