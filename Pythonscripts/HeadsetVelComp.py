import pandas as pd
import os


def change_value_df(own_df):
    for index, row in own_df.iterrows():
        if index == len(own_df.index) - 1:
            own_df.at[index, "HeadVel.x"] = own_df.at[index - 1, "HeadVel.x"]
            own_df.at[index, "HeadVel.y"] = own_df.at[index - 1, "HeadVel.y"]
            own_df.at[index, "HeadVel.z"] = own_df.at[index - 1, "HeadVel.z"]
            return
        if own_df.at[index, "HeadVel.x"] == 0:
            own_df.at[index, "HeadVel.x"] = calculate_vel('x', index)
        elif own_df.at[index, "HeadVel.y"] == 0:
            own_df.at[index, "HeadVel.y"] = calculate_vel('y', index)
        elif own_df.at[index, "HeadVel.z"] == 0:
            own_df.at[index, "HeadVel.z"] = calculate_vel('z', index)


def calculate_vel(coord: str, row_index: int) -> float:
    past_point = df.at[row_index, f'HeadPos.{coord}']
    future_point = df.at[row_index + 1, f'HeadPos.{coord}']
    ret = (future_point - past_point) / 0.1
    trunc = float("{:.7f}".format(truncate(ret, 7)))
    return trunc


def truncate(n, decimals=0):
    multiplier = 10 ** decimals
    return int(n * multiplier) / multiplier


if __name__ == '__main__':
    directory = r"PATH TO CSV"

    for root, dirs, files in os.walk(directory):
        for filename in files:
            file_path = os.path.join(root, filename)
            # Perform action on the file
            print(file_path)
            df = pd.read_csv(file_path)
            df.columns = df.columns.str.replace(" ", "")

            change_value_df(df)

            df.to_csv(file_path, index=False)
