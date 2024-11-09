import os
import glob

from selenium import webdriver
from selenium.webdriver.chrome.service import Service
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.common.by import By

# 크롬 드라이버 자동 업데이트
from webdriver_manager.chrome import ChromeDriverManager

# 너무 빠른 로그인으로 인한 자동입력창 방지에 필요한 라이브러리
import time
import pyautogui
import pyperclip


chrome_options = Options()
chrome_options.add_experimental_option("detach", True)

# 블필요한 에러 메세지 없애기
chrome_options.add_experimental_option("excludeSwitches", ["enable-logging"])

# 최신 드라이버 설치 및 설정
service = Service(executable_path=ChromeDriverManager().install())
driver = webdriver.Chrome(service=service, options=chrome_options)

# 웹페이지 해당 주소 이동
driver.implicitly_wait(5) # 웹페이지가 로딩 될 때까지 3초는 기다림
# driver.maximize_window() # 화면 최대화

driver.get("https://translate.google.com/?hl=ko&sl=en&tl=ko&text=Sets%20a%20sticky%20timeout%20to%20implicitly%20wait%20for%20an%20element%20to%20be%20found%2C%20or%20a%20command%20to%20complete.%20This%20method%20only%20needs%20to%20be%20called%20one%20time%20per%20session.%20To%20set%20the%20timeout%20for%20calls%20to%20execute_async_script%2C%20see%20set_script_timeout.&op=translate")

text1 = driver.find_element(By.CSS_SELECTOR, "#yDmH0d > c-wiz > div > div.ToWKne > c-wiz > div.OlSOob > c-wiz > div.ccvoYb > div.AxqVh > div.OPPzxe > div > c-wiz > span > span > div > textarea")


cs_file_path = r'C:\Users\NICEDNB\Desktop\School\2ndSecond\SWproject\unityRepo\KinectAnalyze\Assets\KinectScripts'

# 경로 내 모든 .cs 파일 찾기
cs_files = glob.glob(os.path.join(cs_file_path, "*.cs"))

# 각 .cs 파일에 대해 수정 작업 수행
for file_path in cs_files:
    li = []


    with open(file_path, "r", encoding="utf-8") as file:

        print(file)

        for line in file:

            line_strip = line.strip()

            if line_strip.startswith('////'):
                text1.click()
                pyautogui.hotkey("ctrl", "a")
                time.sleep(0.5)

                pyperclip.copy(line_strip[line_strip.find('////')+4:].strip())
                pyautogui.hotkey("ctrl", "v")
                time.sleep(7) # 복붙 후에 텀을 주기 위해 7초 기다리도록

                text2_all = driver.find_elements(By.CSS_SELECTOR, "#yDmH0d > c-wiz > div > div.ToWKne > c-wiz > div.OlSOob > c-wiz > div.ccvoYb > div.AxqVh > div.OPPzxe > c-wiz > div > div.usGWQd > div > div.lRu31 > span.HwtZe")

                li.append(line.replace(line.strip(), ("// " + ' '.join([text.text for text in text2_all])).replace('\n', '')))



            elif not line_strip.startswith('///') and line_strip.startswith('//'):
                text1.click()
                pyautogui.hotkey("ctrl", "a")
                time.sleep(0.5)

                pyperclip.copy(line_strip[line_strip.find('//')+2:].strip())
                pyautogui.hotkey("ctrl", "v")
                time.sleep(7) # 복붙 후에 텀을 주기 위해 7초 기다리도록

                text2_all = driver.find_elements(By.CSS_SELECTOR, "#yDmH0d > c-wiz > div > div.ToWKne > c-wiz > div.OlSOob > c-wiz > div.ccvoYb > div.AxqVh > div.OPPzxe > c-wiz > div > div.usGWQd > div > div.lRu31 > span.HwtZe")

                li.append(line.replace(line.strip(), ("// " + ' '.join([text.text for text in text2_all])).replace('\n', '')))


            else:
                li.append(line)

    with open(file_path, "w", encoding="utf-8") as file:
        file.writelines(li)

driver.close()