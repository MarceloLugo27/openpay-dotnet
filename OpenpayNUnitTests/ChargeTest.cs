﻿using NUnit.Framework;
using System;
using Openpay;
using Openpay.Entities;
using Openpay.Entities.Request;
using System.Collections.Generic;
using System.Numerics;

namespace OpenpayNUnitTests
{
	[TestFixture ()]
	public class ChargeTest
	{

		[Test()]
		public void TestGetByOrderId()
		{
			string orderId = "mono3-scoti-oid-00006";

			OpenpayAPI openpayAPI = new OpenpayAPI(Constants.NEW_API_KEY, Constants.NEW_MERCHANT_ID);

			SearchParams search = new SearchParams();
			search.OrderId = orderId;
			search.Status = TransactionStatus.REFUNDED;
			search.CreationGte = new DateTime(2016, 7, 7);
            search.CreationLte = new DateTime(2016, 12, 15);
			List<Charge> charges = openpayAPI.ChargeService.List(search);

			Console.WriteLine("charges: " + charges.ToArray());

			Assert.IsNotNull(charges);
			Assert.AreEqual(1, charges.Count);
		}

		[Test()]
		public void TestCharge()
		{
			OpenpayAPI openpayAPI = new OpenpayAPI(Constants.API_KEY, Constants.MERCHANT_ID);
			Card card = openpayAPI.CardService.Create(GetCardInfo());

			ChargeRequest request = new ChargeRequest();
			request.Method = "card";
			request.SourceId = card.Id;
			request.Description = "Testing from .Net";
			request.Amount = new Decimal(111.00);

			Charge charge = openpayAPI.ChargeService.Create(request);
			Assert.IsNotNull(charge);
			Assert.IsNotNull(charge.Id);
			Assert.IsNotNull(charge.CreationDate);
			Assert.AreEqual("completed", charge.Status);
		}

		[Test()]
		public void TestCharge3DSecure()
		{
			OpenpayAPI openpayAPI = new OpenpayAPI(Constants.API_KEY, Constants.MERCHANT_ID);
			Card card = openpayAPI.CardService.Create(GetCardInfo());

			ChargeRequest request = new ChargeRequest();
			request.Method = "card";
			request.SourceId = card.Id;
			request.Description = "Testing from .Net";
			request.Amount = new Decimal(111.00);
			request.Use3DSecure = true;
			request.RedirectUrl = "https://www.openpay.mx";


			Charge charge = openpayAPI.ChargeService.Create(request);
			Assert.IsNotNull(charge);
			Assert.IsNotNull(charge.Id);
			Assert.IsNotNull(charge.CreationDate);
			Assert.IsNotNull(charge.PaymentMethod.Url);
			Assert.AreEqual("charge_pending", charge.Status);
		}

		[Test()]
		public void TestRefundCharge()
		{

			OpenpayAPI openpayAPI = new OpenpayAPI(Constants.NEW_API_KEY, Constants.NEW_MERCHANT_ID);
			Card card = openpayAPI.CardService.Create(GetScotiaCardInfo());

			SearchParams searh = new SearchParams();
			List<Customer> customers = openpayAPI.CustomerService.List(searh);
			Customer customer = customers[0];
			customer.ExternalId = null;

			ChargeRequest request = new ChargeRequest();
			request.Method = "card";
			request.SourceId = card.Id;
			request.Description = "Testing from .Net";
			request.Amount = new Decimal(111.00);
			request.DeviceSessionId = "sah2e76qfdqa72ef2e2q";
			request.Customer = customer;

			Charge charge = openpayAPI.ChargeService.Create(request);
			Assert.IsNotNull(charge);
			Assert.IsNotNull(charge.Id);
			Assert.IsNotNull(charge.CreationDate);
			Assert.AreEqual("completed", charge.Status);

			Decimal refundAmount = charge.Amount;
			string description = "reembolso desce .Net de " + refundAmount;

			Charge refund = openpayAPI.ChargeService.Refund(charge.Id, description, refundAmount);

			Assert.IsNotNull(refund);
			Assert.IsNotNull(refund.Id);
			Assert.IsNotNull(refund.CreationDate);
			Assert.AreEqual("completed", refund.Status);
		}

		[Test()]
		public void TestRedirectCharge()
		{

			OpenpayAPI openpayAPI = new OpenpayAPI(Constants.NEW_API_KEY, Constants.NEW_MERCHANT_ID);

			SearchParams searh = new SearchParams();
			List<Customer> customers = openpayAPI.CustomerService.List(searh);
			Customer customer = customers[0];
			customer.ExternalId = null;

			ChargeRequest request = new ChargeRequest();
			request.Method = "card";
			request.Description = "Testing redirect from .Net";
			request.Amount = new Decimal(111.00);
			request.DeviceSessionId = "sah2e76qfdqa72ef2e2q";
			request.Customer = customer;
			request.Confirm = false;
			request.SendEmail = true;
			request.RedirectUrl = "http://www.openpay.mx/index.html";

			Charge charge = openpayAPI.ChargeService.Create(request);
			Assert.IsNotNull(charge);
			Assert.IsNotNull(charge.Id);
			Assert.IsNotNull(charge.CreationDate);
			Assert.AreEqual("charge_pending", charge.Status);
			Console.WriteLine("Url: "+ charge.PaymentMethod.Url);

		}

		[Test()]
		public void TestPointsBalance()
		{
			//string customerExternaiId = "monos003_customer001";
			String customer_id = "asda4znfoxhvpgcsui3q";
			String card_id = "keqctdqbro2b7jtcnz7d";

			OpenpayAPI openpayAPI = new OpenpayAPI(Constants.NEW_API_KEY, Constants.NEW_MERCHANT_ID);
			//Card card = openpayAPI.CardService.Create(GetScotiaCardInfo());

			PointsBalance pointsBalance = openpayAPI.CardService.Points(customer_id,card_id);

			Assert.IsNotNull(pointsBalance);
			Assert.AreEqual(new BigInteger(450), pointsBalance.RemainingPoints);
			Assert.AreEqual(new Decimal(33.75), pointsBalance.RemainingMxn);
			Assert.AreEqual(PointsType.BANCOMER, pointsBalance.PointsTypeEnum);

			Console.WriteLine("pointsBalance:[remaining Mx:" + pointsBalance.RemainingMxn + ", remainingPoints:" + pointsBalance.RemainingPoints + "]");

		}

		[Test ()]
		public void TestSantanderPoints ()
		{
			OpenpayAPI openpayAPI = new OpenpayAPI(Constants.API_KEY, Constants.MERCHANT_ID);
			Card card = openpayAPI.CardService.Create (GetCardInfo ());

			ChargeRequest request = new ChargeRequest();
			request.Method = "card";
			request.SourceId = card.Id;
			request.Description = "Testing from .Net";
			request.Amount = new Decimal(215.00);
			request.UseCardPoints = UseCardPointsType.MIXED.ToString();

			Charge charge = openpayAPI.ChargeService.Create(request);
			Assert.IsNotNull(charge);
			Assert.IsNotNull(charge.Id);
			Assert.IsNotNull(charge.CreationDate);
			Assert.AreEqual("completed", charge.Status);
		}

        public void TestChargeWithAffiliation()
        {
            OpenpayAPI openpayAPI = new OpenpayAPI(Constants.API_KEY, Constants.MERCHANT_ID);
            Card card = openpayAPI.CardService.Create(GetCardInfo());

            ChargeRequest request = new ChargeRequest();
            request.Method = "card";
            request.SourceId = card.Id;
            request.Description = "Testing from .Net";
            request.Amount = new Decimal(215.00);

            Affiliation affiliation = new Affiliation();
            affiliation.Name = "amex_3d";
            request.Affiliation = affiliation;

            Charge charge = openpayAPI.ChargeService.Create(request);
            Assert.IsNotNull(charge);
            Assert.IsNotNull(charge.Id);
            Assert.IsNotNull(charge.CreationDate);
            Assert.AreEqual("completed", charge.Status);
        }

		public Card GetCardInfo()
		{
			Card card = new Card();
			card.CardNumber = "5555555555554444";
			card.HolderName = "Juanito Pérez Nuñez";
			card.Cvv2 = "132";
			card.ExpirationMonth = "12";
			card.ExpirationYear = "20";
			return card;
		}

		public Card GetScotiaCardInfo()
		{
			Card card = new Card();
			card.CardNumber = "5105105105105100";
			card.HolderName = "Aquiles Salto Ramon";
			card.Cvv2 = "123";
			card.ExpirationMonth = "12";
			card.ExpirationYear = "21";
			return card;
		}

	}
}

