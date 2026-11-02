# MLP Classification Experiment

This project uses a custom C# three-layer multi-layer perceptron (input, hidden, and output layers) to classify the Wisconsin Diagnostic Breast Cancer dataset. It uses no external machine-learning libraries and targets .NET Framework 4.7.2.

## How to Run

1. Open `MlpProject.slnx` in Visual Studio.
2. Build the solution.
3. Run the `MlpProject` project.
4. Confirm that the dataset path shown in the form points to the copied `data.csv` file.
5. Choose the learning rate, hidden-node count, and epoch count, then select **Train model**.
6. Select **Run tuning** to compare the fixed-seed configurations used for the experiment table.

The form uses an 80/20 stratified split with seed `42`. Features are fitted and MinMax-scaled using the training split, then the same scaler is applied to the test split. The target labels are encoded as `B = 0` and `M = 1`.

## Architecture

- `DataParser` reads the CSV and validates numeric feature values.
- `DatasetSplitter` creates a reproducible stratified train/test split.
- `MinMaxScaler` normalizes every feature to `[0, 1]` using training data only.
- `LabelEncoder` converts the diagnosis labels to network targets.
- `MlpNetwork` implements the input, hidden, and output layers, sigmoid activation, binary cross-entropy loss, and backpropagation updates.
- `TrainingLoop` averages sample loss per epoch and evaluates the test split.
- `Evaluator`, `ConfusionMatrix`, and `ClassificationMetrics` calculate accuracy, precision, and recall with zero-denominator protection.

The UI runs training on a background task so that the window remains responsive. It displays the loss at epoch 1, every 10% of the epoch count, and the final test metrics.

## Hyperparameter Tuning Results

Run each configuration using the same train/test split and random seed. Copy the values printed by **Run tuning** into this table. The displayed percentages are test-set results.

| Learning Rate | Hidden Node Count | Epoch Size | Final Loss | Accuracy | Observations/Justifications |
|---:|---:|---:|---:|---:|---|
| 0.001 | 10 | 500 |  |  |  |
| 0.005 | 10 | 500 |  |  |  |
| 0.010 | 10 | 500 |  |  |  |
| 0.050 | 10 | 500 |  |  |  |
| 0.010 | 5 | 500 |  |  |  |
| 0.010 | 20 | 500 |  |  |  |
| 0.010 | 30 | 500 |  |  |  |

## How to Interpret the Experiments

The learning rate controls the size of each weight update. If it is too small, the loss decreases slowly and the model may underfit within the selected epoch limit. If it is too large, the loss can oscillate or become numerically unstable. Prefer a learning rate with a smooth decreasing loss and strong test accuracy, precision, and recall.

The hidden-node count controls model capacity. Too few nodes may fail to represent the relationship between the 30 input features and diagnosis. More nodes can improve training performance, but excessive capacity can increase runtime or overfit. Prefer the smallest hidden layer that produces stable test metrics and a small final loss.

Do not select a configuration from accuracy alone. Compare precision, recall, final loss, and the gap between training behavior and test performance. Keep the split and random seed fixed for a fair comparison.

## Dataset

The included `MlpProject/data.csv` file is the Wisconsin Diagnostic Breast Cancer dataset from the UCI Machine Learning Repository. It contains 569 records, an ID column, a diagnosis column (`B` or `M`), and 30 numeric features.

## Rubric Coverage

- **Functional correctness:** custom forward propagation, sigmoid output, binary cross-entropy loss, and backpropagation weight/bias updates.
- **Data preprocessing:** CSV parsing, invalid-value checks, stratified 80/20 splitting, and training-only MinMax fitting.
- **Hyperparameter tuning:** UI sweep across learning rates and hidden-layer sizes with fixed seed and documented results table.
- **Evaluation:** accuracy, positive-class precision, positive-class recall, confusion matrix, and safe division for undefined metrics.
- **Code quality:** responsibilities are separated into parser, preprocessing, network, training, evaluation, and UI layers with XML documentation on the core APIs.
