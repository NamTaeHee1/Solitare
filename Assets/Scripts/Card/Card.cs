using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
	[SerializeField] private Sprite CardFrontTexture;
	[SerializeField] private Sprite CardBackTexture;

	public CardEnum.CardState CardState = CardEnum.CardState.IDLE;
	public CardEnum.CardDirection CardTextureDIrection = CardEnum.CardDirection.BACK;
	public Card pCard = null; // Parent Card

	[SerializeField] private Vector3 ChildCardPosition = Vector3.zero;
	[SerializeField] private RectTransform CardRect;

	// 기호 정보 변수 ex) 킹, 퀸, 다이아몬드

	// 숫자 정보 변수 ex) 1 ~ 9

	#region Propety, Init
	public string CardName 
	{
		get { return CardName; }
		set { transform.name = value; }
	}

	public void SetCardInfo(Sprite CardFrontTexure, string CardName)
	{
		this.CardFrontTexture = CardFrontTexure;
		this.CardName = CardName;
	}

	private void SetCardState(CardEnum.CardState state) => CardState = state;
	#endregion

	#region Texture
	public void Show(CardEnum.CardDirection Direction)
	{
		switch (Direction)
		{
			case CardEnum.CardDirection.FRONT:
				GetComponent<Image>().sprite = CardFrontTexture;
				break;
			case CardEnum.CardDirection.BACK:
				GetComponent<Image>().sprite = CardBackTexture;
				break;
		}

		CardTextureDIrection = Direction;
	}
	#endregion

	#region Point
	public Point GetCurPoint()
	{
		return transform.parent.GetComponent<Point>();
	}
	#endregion

	#region IHandler Functions
	public void OnPointerDown(PointerEventData eventData)
	{
		SetCardState(CardEnum.CardState.CLICKED);
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (CardTextureDIrection == CardEnum.CardDirection.BACK)
			return;
		SetCardState(CardEnum.CardState.DRAGING);
		CardRect.anchoredPosition = Vector2.Lerp(CardRect.anchoredPosition, CardRect.anchoredPosition + eventData.delta, 1.0f);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (CardTextureDIrection == CardEnum.CardDirection.BACK)
			return;
		SetCardState(CardEnum.CardState.IDLE);
		Move();
	}
	#endregion

	#region Move & Drag Function
	public void Move(Point movePoint = null, float WaitTime = 0)
	{
		if(movePoint == null) // 플레이어가 드래그하고 PointerUp 함수가 호출 될 경우
		{
			if (pCard != null)
				StartCoroutine(MoveCard(pCard.transform.localPosition + ChildCardPosition, WaitTime));
			else
				StartCoroutine(MoveCard(Vector3.zero, WaitTime));

			return;
		}

		List<Card> OverlapCards = SearchCardAround();

		foreach (Card card in OverlapCards)
		{
			if (card.CardTextureDIrection == CardEnum.CardDirection.BACK // 뒷면이거나
				/* 카드 규칙에 맞지 않거나 */) 
					OverlapCards.Remove(card);
		}

		for (int i = 0; i < OverlapCards.Count; i++)
			Debug.Log($"CardName : {transform.name}, OverlapCards[{i}] : {OverlapCards[i].name}");

		// 스크립트에서 Move 함수를 호출할 경우
		if (movePoint.GetChildCount() == 0) // 이동할 Point에 아무 카드도 없다면
		{
			transform.SetParent(movePoint.transform);
			StartCoroutine(MoveCard(Vector3.zero, WaitTime));
		}
		else // 있다면
		{ // pCard에 값을 넣어도 초기화됨
			transform.SetParent(movePoint.transform);
			pCard = movePoint.transform.GetChild(transform.GetSiblingIndex() - 1).GetComponent<Card>();
			StartCoroutine(MoveCard(ChildCardPosition * (movePoint.GetChildCount() - 1), WaitTime));
		}
	}

	IEnumerator MoveCard(Vector3 ToPos, float WaitTime = 0)
	{
		float t = 0;
		float toPosTime = 0.75f;
		yield return new WaitForSeconds(WaitTime);
		while (toPosTime > t)
		{
			if (CardState == CardEnum.CardState.CLICKED)
				break;
			t += Time.deltaTime;
			CardRect.localPosition = Vector2.Lerp(CardRect.localPosition, ToPos, t / toPosTime);
			yield return null;
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(transform.localPosition, new Vector2(2, 2));
	}

	private List<Card> SearchCardAround() // 주변 카드 검색 및 리스트로 반환 & pCard로 지정하는 함수는 따로 구현
	{
		Collider2D[] OverlapObjects = Physics2D.OverlapBoxAll(transform.position, CardRect.sizeDelta, 0);

		for (int i = 0; i < OverlapObjects.Length; i++)
			Debug.Log(OverlapObjects[i].name);

		List<Card> OverlapCards = new List<Card>();

		foreach (Collider2D Object in OverlapObjects)
		{
			if (Object.CompareTag("Card"))
				OverlapCards.Add(Object.GetComponent<Card>());
		}

		return OverlapCards;
	}
	#endregion
}