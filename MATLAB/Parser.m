%% Read data
clear; 
close all; 
clc;
path = '../Output/';
file = 'AllStats_Output.csv';
data = xlsread([path  file]);

info = data(1,:);
info(isnan(info)) = [];
algInfo.inputs = info(1);
algInfo.outputs = info(2);
algInfo.maximums = info(3:2+length(algInfo.outputs));
algInfo.normal = info(3+length(algInfo.outputs));

movementData = data(2,:);
movementData(isnan(movementData)) = [];

globalUpdateVal = data(3,:);
globalUpdateVal(isnan(globalUpdateVal)) = [];

globalUpdateIter = data(4,:);
globalUpdateIter(isnan(globalUpdateIter)) = [];

% realOutputs = data(5,:);
% realOutputs(isnan(realOutputs)) = [];
% realOutputs = reshape(realOutputs, [algInfo.outputs length(realOutputs)/algInfo.outputs]);

% networkOutputs = data(6,:);
% networkOutputs(isnan(networkOutputs)) = [];
% networkOutputs = reshape(networkOutputs, [algInfo.outputs length(networkOutputs)/algInfo.outputs]);

% maximums = repmat(algInfo.maximums', [1 length(networkOutputs)]);
% if (algInfo.normal == 1)
%     realOutputs = realOutputs .* maximums;
%     networkOutputs = networkOutputs .* maximums;
% end

%% Plot results
figure; 
subplot(2,1,1); plot(movementData); title('Average Movement');
subplot(2,1,2); plot(globalUpdateIter,globalUpdateVal,'-*'); title('Global Updates');
xlim([1 length(movementData)]);
[sortNetworkOutputs sortNetworkIdx] = sort(networkOutputs, 2);
[sortRealOutputs sortRealIdx] = sort(realOutputs, 2);
useRealBaseline = false;

% Get moving average and moving difference average
len = 5;
diff = zeros(algInfo.outputs, size(realOutputs,2));
avg = zeros(algInfo.outputs, size(realOutputs,2));
stdDev = zeros(algInfo.outputs, size(realOutputs,2));
for i = 1:algInfo.outputs
    trendData = realOutputs(i,sortNetworkIdx(i,:));
    for j = 1:size(realOutputs,2)
        if (j <= len)
            avg(i,j) = mean(trendData(1:j+len));
            stdDev(i,j) = sqrt(sum((trendData(1:j+len)-avg(i,j)).^2)/avg(i,j));
            diff(i,j) = mean(abs(realOutputs(i, 1:j+len) - networkOutputs(i, 1:j+len)));
        elseif (j + len > size(realOutputs,2))
            avg(i,j) = mean(trendData(j-len:end));
            stdDev(i,j) = sqrt(sum((trendData(j-len:end)-avg(i,j)).^2)/avg(i,j));
            diff(i,j) = mean(abs(realOutputs(i, j-len:end) - networkOutputs(i, j-len:end)));
        else
            avg(i,j) = mean(trendData(j-len:j+len));
            stdDev(i,j) = sqrt(sum((trendData(j-len:j+len)-avg(i,j)).^2)/avg(i,j));
            diff(i,j) = mean(abs(realOutputs(i, j-len:j+len) - networkOutputs(i, j-len:j+len)));
        end
    end
%     figure; plot(diff(i,:)); title('Moving Difference');
end

% Plot
for i = 1:1%algInfo.outputs
    if (useRealBaseline)
        figure;
        scatter(1:length(realOutputs(i,:)), sortRealOutputs(i,:), 'g');
        strTitle = sprintf('Output %d', i); title(strTitle);
        hold on; grid on;
        scatter(1:length(networkOutputs(i,:)), networkOutputs(i,sortRealIdx(i,:)), 'b');
        scatter(1:length(realOutputs), avg(i,:), 'm');
        scatter(1:size(stdDev(i,:),2), stdDev(i,:));
        legend('Real','Network','Moving Average','Standard Deviation');
        hold off;
    else
        figure;
        scatter(1:length(networkOutputs(i,:)), sortNetworkOutputs(i,:));
        strTitle = sprintf('Output %d', i); title(strTitle);
        hold on; grid on;
        scatter(1:length(realOutputs(i,:)), realOutputs(i,sortNetworkIdx(i,:)), 'g');
        scatter(1:length(realOutputs), avg(i,:), 'm');
        plot(stdDev(i,:));
        legend('Network','Real','Moving Average','Standard Deviation');
        hold off;
    end
    fprintf('Moving Average Error:   %f\n', mean(abs(avg(i,:) - sortNetworkOutputs(i,:))));
    fprintf('Standard Deviation:     %f\n\n', mean(stdDev(i,:)));
end

%% Winner Accuracy
path = '../Output/';
file = 'Point_Predictions.csv';
ptPredictions = xlsread([path  file]);
ptPredictions(any(isnan(ptPredictions),2),:) = [];

simulatedPts = ptPredictions(:,1:2);
actualPts = ptPredictions(:,3:4);

[homePts homeIdx] = sort(ptPredictions(:,1));
[visitorPts visitorIdx] = sort(ptPredictions(:,2));
realHome = ptPredictions(:,3);
realVisitor = ptPredictions(:,4);

figure;
subplot(1,2,1); scatter(1:length(homePts), homePts); hold on;
subplot(1,2,1); scatter(1:length(homePts), realHome(homeIdx));
subplot(1,2,2); scatter(1:length(visitorPts), visitorPts); hold on;
subplot(1,2,2); scatter(1:length(visitorPts), realVisitor(visitorIdx));

fprintf('Home Bias:    %0.2f\n', sum(homePts - realHome(homeIdx))/length(homePts));
fprintf('Visitor Bias: %0.2f\n\n', sum(visitorPts - realVisitor(visitorIdx))/length(visitorPts));

fprintf('Home Error:    %0.2f\n', sum(abs(homePts - realHome(homeIdx)))/length(homePts));
fprintf('Visitor Error: %0.2f\n\n', sum(abs(visitorPts - realVisitor(visitorIdx)))/length(visitorPts));

% Get gross accuracy
simHomeWin = min(simulatedPts(:,1)>simulatedPts(:,2),1);
actHomeWin = min(actualPts(:,1)>actualPts(:,2),1);
correct = actHomeWin == simHomeWin;
fprintf('%0.2f%% correct\n\n', 100 * mean(correct));

% Plot gross accuracy
[homePtsSort homePtIdx] = sort(simulatedPts(:,1));
homePtsCorrect = homePtsSort .* correct(homePtIdx);
homePtsIncorrect = homePtsSort .* (1 - correct(homePtIdx));
% figure;
% s2 = scatter(1:length(homePtsSort), homePtsCorrect);
% set(s2, 'SizeData', 100);
% hold on;
% s1 = scatter(1:length(homePtsSort), homePtsIncorrect, 'r', 'MarkerFaceColor','r');
% set(s1, 'SizeData', 5);
% title('Game Prediction Accuracy');

% Get point differential vs accuracy
simScoreDif = simulatedPts(:,1) - simulatedPts(:,2);
actScoreDif = actualPts(:,1) - actualPts(:,2);

onlyAbove = 0;
actScoreDif = actScoreDif(abs(simScoreDif) > onlyAbove);
simScoreDif = simScoreDif(abs(simScoreDif) > onlyAbove);

mseDif = sum((simScoreDif - actScoreDif).^2) / length(simScoreDif);
fprintf('All Mean Squared Error: %f\n', mseDif);
avgDif = sum(abs(simScoreDif - actScoreDif)) / length(simScoreDif);
fprintf('All Average Error:      %f\n\n', avgDif);
correct = (simScoreDif./abs(simScoreDif)) == (actScoreDif./abs(actScoreDif));

% Plot point differential vs accuracy
[simDifSort simDifIdx] = sort(simScoreDif);
simDifCorrect = simDifSort .* correct(simDifIdx);
simDifIncorrect = simDifSort .* (1 - correct(simDifIdx));
% figure;
% s1 = scatter(1:length(simDifSort), simDifCorrect);
% set(s1, 'SizeData', 100);
% hold on;
% s2 = scatter(1:length(simDifSort), simDifIncorrect, 'r', 'MarkerFaceColor','r');
% set(s2, 'SizeData', 5);
% title('Game Difference Accuracy');

[simDifSort simDifIdx] = sort(simScoreDif);
simDifCorrect = simDifSort .* correct(simDifIdx);
actScoreDifCor = actScoreDif(simDifIdx); 
actScoreDifCor = actScoreDifCor(correct(simDifIdx) == 1);

mseDifCor = sum((simDifCorrect(correct(simDifIdx) == 1) - actScoreDifCor).^2) / length(simScoreDif(correct(simDifIdx) == 1));
fprintf('Cor Mean Squared Error: %f\n', mseDifCor);
avgDifCor = sum(abs(simDifCorrect(correct(simDifIdx) == 1) - actScoreDifCor)) / length(simScoreDif(correct(simDifIdx) == 1));
fprintf('Cor Average Error:      %f\n\n', avgDifCor);

simDifIncorrect = simDifSort .* (1 - correct(simDifIdx));
actScoreDifInc = actScoreDif(simDifIdx); 
actScoreDifInc = actScoreDifInc(correct(simDifIdx) == 0);

mseDifInc = sum((simDifIncorrect(correct(simDifIdx) == 0) - actScoreDifInc).^2) / length(simScoreDif(correct(simDifIdx) == 0));
fprintf('Inc Mean Squared Error: %f\n', mseDifInc);
avgDifInc = sum(abs(simDifIncorrect(correct(simDifIdx) == 0) - actScoreDifInc)) / length(simScoreDif(correct(simDifIdx) == 0));
fprintf('Inc Average Error:      %f\n\n', avgDifInc);

figure;
s1 = scatter(1:length(simDifSort), simDifCorrect);
set(s1, 'SizeData', 100);
hold on;
s2 = scatter(1:length(simDifSort), simDifIncorrect, 'r', 'MarkerFaceColor','r');
set(s2, 'SizeData', 5);
s3 = scatter(1:length(simDifSort), actScoreDif(simDifIdx), 'g');
title('Game Difference Accuracy');
legend('Correct Prediction','Incorrect Prediction','Actual');

figure;
[actDifSort actDifIdx] = sort(actScoreDif);
s1 = scatter(1:length(actDifSort), simScoreDif(actDifIdx).*correct(actDifIdx));
hold on;
s2 = scatter(1:length(actDifSort), simScoreDif(actDifIdx).*(1-correct(actDifIdx)), 'r');
s3 = scatter(1:length(actDifSort), actDifSort, 'g');
legend('Correct Prediction','Incorrect Prediction','Actual');

sortCorrect = correct(simDifIdx);
binSize = 20;
probBins = zeros(2,binSize);
avgDif = zeros(1,binSize);
for i = 0:binSize-1
    startIdx = round((i * length(simDifSort)) / binSize + 1);
    endIdx = round(((i+1) * length(simDifSort)) / binSize);
    correctData = simDifSort(startIdx:endIdx);
    avgDif(i+1) = mean(correctData);
    correctData = correctData .* sortCorrect(startIdx:endIdx);
    probBins(1,i+1) = length(correctData(correctData ~= 0)) / length(correctData);
    probBins(2,i+1) = length(correctData(correctData == 0)) / length(correctData);
end
% figure; bar(probBins(1,:)); title('Percent Accurate vs Simulated Differential');
figure; bar(probBins(2,:)); title('Percent Inaccurate vs Simulated Differential');
hold on; 
plot(1:binSize, abs(avgDif./max(abs(avgDif))), '-o', 'LineWidth', 2, 'Color', 'g');
grid on; grid minor; xlim([0 binSize+1]); hold off;

