import pandas as pd
from sklearn.preprocessing import OneHotEncoder, StandardScaler
from sklearn.compose import ColumnTransformer
from sklearn.pipeline import Pipeline
from sklearn.base import BaseEstimator, TransformerMixin

class ConditionalOneHotEncoder(BaseEstimator, TransformerMixin):
    def __init__(self):
        self.encoder = OneHotEncoder(sparse=False, handle_unknown='ignore')
    
    def fit(self, X, y=None):
        tags = X['league tags'][X['joined_league']].apply(pd.Series).stack().unique()
        self.encoder.fit(tags.reshape(-1, 1))
        return self
    
    def transform(self, X):
        tags_transformed = self.encoder.transform(X.loc[X['joined_league'], 'league tags'].explode().unique().reshape(-1, 1))
        
        tags_df = pd.DataFrame(tags_transformed, columns=self.encoder.get_feature_names_out(['league tags']))
        
        tags_df.index = X.loc[X['joined_league'], 'league tags'].explode().index
        tags_df = tags_df.groupby(tags_df.index).sum()
        
        full_tags_df = pd.DataFrame(0, index=X.index, columns=tags_df.columns)
        full_tags_df.update(tags_df)
        
        return full_tags_df.values


def load_data(data_records):
    df = pd.DataFrame(data_records)
    return df

def process_data(dframe):
    dframe[['wins', 'losses']] = pd.DataFrame(dframe['record'].tolist(), index=dframe.index)

    dframe.loc[~dframe['joined_league'], 'league tags'] = dframe.loc[~dframe['joined_league'], 'league tags'].apply(lambda x: [])

    scaler = StandardScaler()
    one = OneHotEncoder()

    preprocessor = ColumnTransformer(
        transformers=[
            ('cat', OneHotEncoder(), ['Username', 'League']),
            ('bin', 'passthrough', ['joined_league']),
            ('tags', ConditionalOneHotEncoder(), 'league tags')
        ]
    )
    
    pipeline = Pipeline(steps=[
        ('preprocessor', preprocessor),
        ('scaler', scaler)
    ])

    X = pipeline.fit_transform(dframe)

    y = dframe['Username']
    
    return dframe, X, y
