
def win_loss_ratio(wins, losses):
    return wins / (wins + losses) if (wins + losses) > 0 else 0

def find_common_tags(records):
    all_tags = []
    for record in records:
        all_tags.append(record.LeagueTags)
    
    unique_tags = set()
    answer = []
    for tag in all_tags:
        for tag_item in tag:
            if tag_item not in unique_tags:
                answer.append(tag_item)
                unique_tags.add(tag_item)
    
    return answer


def extract_features(record, all_tags):
    user_wins, user_losses = 0, 0
    if 'leaguePlayerOverallRecord' in record:
        user_wins, user_losses = record.leaguePlayerOverallRecord
        user_ratio = win_loss_ratio(user_wins, user_losses)

    combined_wins = sum(r[0] for r in record.LeagueIndividualOverallRecord)
    combined_losses = sum(r[1] for r in record.LeagueIndividualOverallRecord)
    combined_ratio = win_loss_ratio(combined_wins, combined_losses)

    relative_performance = user_ratio - combined_ratio if record.leaguePlayerOverallRecord is None else 0

    tag_vector = [1 if tag in record.LeagueTags else 0 for tag in all_tags]

    features = [relative_performance, combined_ratio] + tag_vector

    return features


