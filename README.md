# MLP Classification Experiment

This project uses a custom C# three-layer multi-layer perceptron (input, hidden, and output layers) to classify the Wisconsin Diagnostic Breast Cancer dataset. It uses no external machine-learning libraries and targets .NET Framework 4.7.2.

## How to Run

1. Open `MlpProject.slnx` in a recent Visual Studio (2022 17.13 or later, which supports `.slnx`), or open `MlpProject/MlpProject.csproj` directly.
2. Build the solution.
3. Run the `MlpProject` project.
4. Confirm that the dataset path shown in the form points to the copied `data.csv` file.
5. Choose the learning rate, hidden-node count, and epoch count, then select **Train model**.
6. Select **Run tuning** to compare the fixed-seed configurations used for the experiment table.
7. Use **Results folder** to open the exported logs (see *Training Log and Outputs*).

**Important:** If `Solution 'MlpProject'` shows `(0 projects)` or the Start button displays **Attach**:

1. Right-click `Solution 'MlpProject'`.
2. Select **Add > Existing Project**.
3. Select `MlpProject > MlpProject > MlpProject.csproj`.
4. Right-click the added `MlpProject` project and select **Set as Startup Project**.
5. Click the Start button to run the application.

The form uses an 80/20 stratified split with seed `42`. Features are fitted and MinMax-scaled using the training split, then the same scaler is applied to the test split. The target labels are encoded as `B = 0` and `M = 1`.

## Architecture

- `DataParser` reads the CSV and validates numeric feature values.
- `DatasetSplitter` creates a reproducible stratified train/test split.
- `MinMaxScaler` normalizes every feature to `[0, 1]` using training data only.
- `LabelEncoder` converts the diagnosis labels to network targets.
- `MlpNetwork` implements the input, hidden, and output layers, sigmoid activation, binary cross-entropy loss, and backpropagation updates.
- `TrainingLoop` averages sample loss per epoch and evaluates the test split.
- `Evaluator`, `ConfusionMatrix`, and `ClassificationMetrics` calculate accuracy, precision, and recall with zero-denominator protection.
- `ExperimentRunner` runs one configuration or the tuning sweep and picks the best configuration.
- `ResultsExporter` writes the training log and summaries to CSV/text files.
- `LossChartPanel` is a dependency-free GDI+ line chart of loss per epoch.

The UI runs training on a background task so that the window remains responsive. It prints the loss at epoch 1, every 100 epochs, and the final epoch, then shows the final train/test metrics and the test confusion matrix, and plots the loss curve.

## Training Log and Outputs

Every run automatically saves its evidence to the `results/` folder next to `MlpProject.csproj` (commit this folder):

| File | Contents |
|------|----------|
| `train_loss_history.csv` / `tuning_loss_history.csv` | Loss after **every** epoch for each configuration (`LearningRate,HiddenNodes,Epochs,Epoch,Loss`) |
| `train_summary.csv` / `tuning_summary.csv` | Final loss, train accuracy, test accuracy, precision, recall, and confusion-matrix counts |
| `train_console_log.txt` / `tuning_console_log.txt` | The exact text shown in the output window, including the loss printed every 100 epochs |

## Visualization

The lower half of the window plots training loss against epoch (one line per configuration during tuning):

```
<img width="805" height="391" alt="loss_curves" src="https://github.com/user-attachments/assets/a86d0e15-8467-4e72-bbe0-3188ca66a7ea" />
```

## Hyperparameter Tuning Results

All runs use the same 80/20 stratified split (357 B / 212 M -> 456 train, 113 test) and seed `42`. Accuracy is on the **test set**. Full per-epoch losses are in `results/tuning_loss_history.csv` and the per-run metrics in `results/tuning_summary.csv`.

| Learning Rate | Hidden Node Count | Epoch Size | Final Loss | Accuracy | Observations/Justifications |
|--------------:|------------------:|-----------:|-----------:|---------:|-----------------------------|
| 0.001         | 10                | 500        | 0.114881   | 96.46%   | Slowest learner: loss never dropped below 0.1 and was still falling (about 0.014 over the last 100 epochs). Underfitting for this epoch budget; it would need many more epochs. |
| 0.005         | 10                | 500        | 0.063249   | 97.35%   | Reached loss < 0.1 at epoch 137. Much better than 0.001 with the same test accuracy as the best run, but still converging slowly. |
| 0.010         | 10                | 500        | 0.052948   | 96.46%   | Reached loss < 0.1 at epoch 70 and then improved slowly (about 0.003 over the last 100 epochs). Smooth, stable curve. |
| 0.050         | 10                | 500        | 0.030798   | 97.35%   | Lowest loss and highest training accuracy (98.90%). Loss fell below 0.1 by epoch 17 and never increased from one epoch to the next, so there was no oscillation or instability at this rate. |
| 0.010         | 5                 | 500        | 0.052783   | 96.46%   | Nearly identical to 10 hidden nodes at the same learning rate. 5 nodes are enough for this data. |
| 0.010         | 20                | 500        | 0.054003   | 96.46%   | No gain over 10 nodes; slightly higher loss and more computation. |
| 0.010         | 30                | 500        | 0.053803   | 96.46%   | No gain over 10 nodes. Extra capacity did not help, and the test results did not change. |

### Optimal configuration

**Chosen:** learning rate = **0.05**, hidden nodes = **10**, epochs = **500**.

**Why:**
- **Learning rate was the dominant factor.** Raising it from 0.001 to 0.05 cut the final loss from 0.115 to 0.031 and sped up convergence enormously. Hidden-layer size (5, 10, 20, 30 at LR 0.01) changed the final loss by less than 0.002, so it had almost no effect on this problem.
- **Best fit without instability.** LR 0.05 gave the lowest loss, the highest training accuracy (98.90%), and the joint-best test accuracy (97.35%, tied with LR 0.005). Its loss decreased every epoch.
- **No sign of overfitting.** Training accuracy (98.90%) is only about 1.5 points above test accuracy (97.35%).
- **Ties:** test accuracy and recall are the same for several runs, so LR 0.05 was chosen by lower loss. Because 5 to 30 hidden nodes performed the same at LR 0.01, the smaller layers are a reasonable choice for speed.

**Caveat:** configurations are ranked on the same test split they are reported on, so the best test accuracy is slightly optimistic. With only 113 test samples, the difference between 97.35% (110 correct) and 96.46% (109 correct) is a single sample, so it is not statistically meaningful.

## Results Summary

Best configuration (LR 0.05, 10 hidden nodes, 500 epochs), test set, positive class = malignant (M):

| Metric | Value |
|--------|-------|
| Accuracy  | 97.35% (110 / 113) |
| Precision (malignant) | 97.56% |
| Recall (malignant)    | 95.24% |
| True positives / False positives | 40 / 1 |
| False negatives / True negatives | 2 / 70 |

|            | Predicted B | Predicted M |
|------------|------------:|------------:|
| **Actual B** | 70 | 1 |
| **Actual M** | 2  | 40 |

Interpretation: the model correctly identified 40 of 42 malignant tumors and 70 of 71 benign ones. The more serious error is the 2 false negatives (malignant cases predicted benign), which is why recall (95.24%) is lower than precision (97.56%). Every configuration produced the same 2 false negatives, which suggests those two samples are hard cases near the decision boundary rather than the result of a poor setting. Ways to reduce them would be training longer at LR 0.05 (the loss was still decreasing at epoch 500), lowering the decision threshold below 0.5 to favor recall, or weighting the malignant class more heavily.

A longer run (LR 0.01, 10 hidden nodes, 1000 epochs, from **Train model**) reached loss 0.045341, 98.46% train accuracy and 97.35% test accuracy; see `results/train_console_log.txt`.

## How to Interpret the Experiments

The learning rate controls the size of each weight updatgit statuse. If it is too small, the loss decreases slowly and the model may underfit within the selected epoch limit. If it is too large, the loss can oscillate or become numerically unstable. Prefer a learning rate with a smooth decreasing loss and strong test accuracy, precision, and recall.

The hidden-node count controls model capacity. Too few nodes may fail to represent the relationship between the 30 input features and diagnosis. More nodes can improve training performance, but excessive capacity can increase runtime or overfit. Prefer the smallest hidden layer that produces stable test metrics and a small final loss.

Do not select a configuration from accuracy alone. Compare precision, recall, final loss, and the gap between training behavior and test performance. Keep the split and random seed fixed for a fair comparison.

## Dataset

The included `MlpProject/data.csv` file is the Wisconsin Diagnostic Breast Cancer dataset from the UCI Machine Learning Repository. It contains 569 records, an ID column, a diagnosis column (`B` or `M`), and 30 numeric features.

## Rubric Coverage

- **Functional correctness:** custom forward propagation, sigmoid output, binary cross-entropy loss, and backpropagation weight/bias updates.
- **Data preprocessing:** CSV parsing, invalid-value checks, stratified 80/20 splitting, and training-only MinMax fitting.
- **Hyperparameter tuning:** UI sweep across learning rates and hidden-layer sizes with fixed seed, exported loss histories, a results table with observations, and a stated optimal configuration.
- **Evaluation:** train and test accuracy, positive-class precision and recall, confusion matrix, loss-curve plot, and safe division for undefined metrics.
- **Code quality:** responsibilities are separated into parser, preprocessing, network, training, evaluation, and UI layers with XML documentation on the core APIs.

## Notes on Requirements

The network (`MlpNetwork.cs`) is implemented from scratch in C# with input, hidden, and output layers and backpropagation, and uses no external machine-learning libraries. The data set is open-source (UCI WDBC).
